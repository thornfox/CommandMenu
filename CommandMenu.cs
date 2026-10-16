using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("CommandMenu", "ThornFox", "1.0.0")]
    [Description("Admin-only UI menu (2x3 grid) that executes configurable chat commands.")]
    public class CommandMenu : RustPlugin
    {
        #region Constants

        private const string PERM_ADMIN = "commandmenu.admin";
        private const string CHAT_COMMAND = "radmin_menu";
        private const string EXEC_COMMAND = "cmdmenu.exec";

        private const string UI_NAME = "CommandMenuUI";
        private const string UI_INNER = UI_NAME + "_Inner";
        private const string UI_PARENT = "Hud.Menu";

        private const int MAX_COLUMNS = 3;
        private const int MAX_ROWS = 2;
        private const int MAX_BUTTONS = MAX_COLUMNS * MAX_ROWS;

        private const string LOG_FILE = "actions";

        #endregion

        #region State

        private PluginConfig _config;
        private readonly HashSet<ulong> _openMenus = new HashSet<ulong>();

        #endregion

        #region Configuration

        public class CommandEntry
        {
            [JsonProperty("Button Text")]
            public string Text { get; set; }

            [JsonProperty("Chat Command")]
            public string ChatCommand { get; set; }
        }

        public class PluginConfig
        {
            [JsonProperty("Config Version")]
            public string ConfigVersion;

            // Keys kept identical to version 0.1.0 so existing values are preserved on migration.
            [JsonProperty("Menu Anchor Min (e.g., 0.01 0.01 for bottom-left)")]
            public string MenuAnchorMin = "0.01 0.01";

            [JsonProperty("Menu Anchor Max (e.g., 0.11 0.0767 for a small menu)")]
            public string MenuAnchorMax = "0.11 0.0767";

            [JsonProperty("Button Font Size")]
            public int FontSize = 10;

            [JsonProperty("Enable mouse cursor while menu is open (blocks mouse look)")]
            public bool EnableCursor = false;

            [JsonProperty("Show chat confirmation after a button click")]
            public bool ShowChatConfirmation = true;

            [JsonProperty("Write button clicks to log file (oxide/logs/CommandMenu)")]
            public bool LogToFile = true;

            [JsonProperty("List of commands to display (MAX 6 recommended for 2x3 layout)")]
            public List<CommandEntry> Commands;
        }

        private static List<CommandEntry> GetDefaultCommands()
        {
            // Examples only - replace with the admin chat commands used on your server.
            return new List<CommandEntry>
            {
                new CommandEntry { Text = "Vanish", ChatCommand = "vanish" },
                new CommandEntry { Text = "Godmode", ChatCommand = "god" },
                new CommandEntry { Text = "Radar", ChatCommand = "radar" },
                new CommandEntry { Text = "TC add self", ChatCommand = "adminpve tc add self" },
            };
        }

        protected override void LoadDefaultConfig()
        {
            _config = new PluginConfig
            {
                ConfigVersion = Version.ToString(),
                Commands = GetDefaultCommands()
            };
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<PluginConfig>();
                if (_config == null)
                    throw new JsonException("Configuration is empty.");

                MigrateConfig();
                SaveConfig();
            }
            catch (Exception ex)
            {
                // Do not overwrite a broken file: the admin's command list would be lost.
                PrintError($"Configuration file is invalid, using defaults until it is fixed: {ex.Message}");
                LoadDefaultConfig();
            }
        }

        private void MigrateConfig()
        {
            string current = Version.ToString();
            if (_config.ConfigVersion != current)
            {
                PrintWarning($"Updating configuration from version {_config.ConfigVersion ?? "0.1.0"} to {current}.");
                _config.ConfigVersion = current;
            }

            if (_config.Commands == null)
                _config.Commands = GetDefaultCommands();

            if (string.IsNullOrWhiteSpace(_config.MenuAnchorMin))
                _config.MenuAnchorMin = "0.01 0.01";

            if (string.IsNullOrWhiteSpace(_config.MenuAnchorMax))
                _config.MenuAnchorMax = "0.11 0.0767";

            if (_config.FontSize <= 0)
                _config.FontSize = 10;
        }

        protected override void SaveConfig() => Config.WriteObject(_config, true);

        #endregion

        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "You do not have permission to use this menu.",
                ["NoCommands"] = "No valid commands are configured for this menu.",
                ["InvalidButton"] = "This button is no longer valid. Please reopen the menu.",
                ["CommandExecuted"] = "Chat command executed: {0}"
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "Du hast keine Berechtigung, dieses Menü zu verwenden.",
                ["NoCommands"] = "Für dieses Menü sind keine gültigen Befehle konfiguriert.",
                ["InvalidButton"] = "Dieser Button ist nicht mehr gültig. Bitte öffne das Menü erneut.",
                ["CommandExecuted"] = "Chat-Befehl ausgeführt: {0}"
            }, this, "de");
        }

        private void Message(BasePlayer player, string key, params object[] args)
        {
            string message = lang.GetMessage(key, this, player.UserIDString);
            player.ChatMessage(args.Length > 0 ? string.Format(message, args) : message);
        }

        #endregion

        #region Oxide Hooks

        private void Init()
        {
            permission.RegisterPermission(PERM_ADMIN, this);
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
                CuiHelper.DestroyUi(player, UI_NAME);

            _openMenus.Clear();
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            ulong id = player.userID;
            _openMenus.Remove(id);
        }

        #endregion

        #region Commands

        [ChatCommand(CHAT_COMMAND)]
        private void CmdToggleMenu(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;

            if (!HasAccess(player))
            {
                Message(player, "NoPermission");
                return;
            }

            ulong id = player.userID;
            if (_openMenus.Contains(id))
            {
                CloseMenu(player);
                return;
            }

            if (!OpenMenu(player))
                Message(player, "NoCommands");
        }

        [ConsoleCommand(EXEC_COMMAND)]
        private void CcmdExecute(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;

            if (!HasAccess(player))
            {
                Message(player, "NoPermission");
                return;
            }

            // Buttons only pass an index; the command itself is taken from the config (whitelist).
            int index = arg.GetInt(0, -1);
            var commands = GetVisibleCommands();
            if (index < 0 || index >= commands.Count)
            {
                Message(player, "InvalidButton");
                return;
            }

            string chatCommand = "/" + commands[index].ChatCommand.Trim().TrimStart('/');

            LogAction(player, chatCommand);
            rust.RunClientCommand(player, "chat.say", chatCommand);

            if (_config.ShowChatConfirmation)
                Message(player, "CommandExecuted", chatCommand);
        }

        #endregion

        #region Helpers

        private bool HasAccess(BasePlayer player) =>
            permission.UserHasPermission(player.UserIDString, PERM_ADMIN);

        private List<CommandEntry> GetVisibleCommands()
        {
            if (_config?.Commands == null)
                return new List<CommandEntry>();

            return _config.Commands
                .Where(c => c != null
                            && !string.IsNullOrWhiteSpace(c.Text)
                            && !string.IsNullOrWhiteSpace(c.ChatCommand))
                .Take(MAX_BUTTONS)
                .ToList();
        }

        private void LogAction(BasePlayer player, string chatCommand)
        {
            string line = $"{player.displayName} ({player.UserIDString}) clicked {chatCommand}";
            Puts(line);

            if (_config.LogToFile)
                LogToFile(LOG_FILE, line, this);
        }

        #endregion

        #region UI

        private bool OpenMenu(BasePlayer player)
        {
            var commands = GetVisibleCommands();
            if (commands.Count == 0)
                return false;

            CuiHelper.DestroyUi(player, UI_NAME);
            CuiHelper.AddUi(player, BuildMenu(commands));

            ulong id = player.userID;
            _openMenus.Add(id);
            return true;
        }

        private void CloseMenu(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI_NAME);
            ulong id = player.userID;
            _openMenus.Remove(id);
        }

        private CuiElementContainer BuildMenu(List<CommandEntry> commands)
        {
            var elements = new CuiElementContainer();

            const float border = 0.005f;
            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 1" },
                RectTransform = { AnchorMin = _config.MenuAnchorMin, AnchorMax = _config.MenuAnchorMax },
                CursorEnabled = _config.EnableCursor
            }, UI_PARENT, UI_NAME);

            elements.Add(new CuiPanel
            {
                Image = { Color = "1 1 1 0.5" },
                RectTransform = { AnchorMin = $"{border} {border}", AnchorMax = $"{1f - border} {1f - border}" }
            }, UI_NAME, UI_INNER);

            const float rowHeight = 1f / MAX_ROWS;
            const float colWidth = 1f / MAX_COLUMNS;
            const float padding = 0.05f;

            for (int i = 0; i < commands.Count; i++)
            {
                int col = i % MAX_COLUMNS;
                int row = i / MAX_COLUMNS;

                float minX = col * colWidth + colWidth * padding;
                float maxX = (col + 1) * colWidth - colWidth * padding;
                float minY = 1f - (row + 1) * rowHeight + rowHeight * padding;
                float maxY = 1f - row * rowHeight - rowHeight * padding;

                elements.Add(new CuiButton
                {
                    Button = { Command = $"{EXEC_COMMAND} {i}", Color = "1 1 1 1" },
                    RectTransform = { AnchorMin = $"{minX} {minY}", AnchorMax = $"{maxX} {maxY}" },
                    Text =
                    {
                        Text = commands[i].Text,
                        FontSize = _config.FontSize,
                        Align = TextAnchor.MiddleCenter,
                        Color = "0 0 0 1"
                    }
                }, UI_INNER, $"{UI_NAME}_{i}");
            }

            return elements;
        }

        #endregion
    }
}
