﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class LobbyChat
    {
        public static bool IsEnabled = false, Return = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0;
        public static List<int> LobbyPlayers = new List<int>();

        public static void Delay(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                CommandCost(_cInfo, _playerName);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastLobby == null)
                {
                    CommandCost(_cInfo, _playerName);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastLobby;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            DateTime _dt;
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                            if (DateTime.Now < _dt)
                            {
                                _donator = true;
                                int _newDelay = Delay_Between_Uses / 2;
                                if (_timepassed >= _newDelay)
                                {
                                    CommandCost(_cInfo, _playerName);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase550;
                                    if (!Phrases.Dict.TryGetValue(550, out _phrase550))
                                    {
                                        _phrase550 = "{PlayerName} you can only use /lobby once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase550 = _phrase550.Replace("{PlayerName}", _playerName);
                                    _phrase550 = _phrase550.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase550 = _phrase550.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase550), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase550), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            CommandCost(_cInfo, _playerName);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase550;
                            if (!Phrases.Dict.TryGetValue(550, out _phrase550))
                            {
                                _phrase550 = "{PlayerName} you can only use /lobby once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase550 = _phrase550.Replace("{PlayerName}", _playerName);
                            _phrase550 = _phrase550.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase550 = _phrase550.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase550), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase550), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _playerName)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (currentCoins >= Command_Cost)
                {
                    Exec(_cInfo, _playerName);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            if (SetLobby.Lobby_Position != "0,0,0")
            {
                int x, y, z;
                if (Return)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 _position = _player.GetPosition();
                    x = (int)_position.x;
                    y = (int)_position.y;
                    z = (int)_position.z;
                    string _pposition = x + "," + y + "," + z;
                    LobbyPlayers.Add(_cInfo.entityId);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LobbyReturn = _pposition;
                    PersistentContainer.Instance.Save();
                    string _phrase552;
                    if (!Phrases.Dict.TryGetValue(552, out _phrase552))
                    {
                        _phrase552 = "{PlayerName} you can go back by typing /return when you are ready to leave the lobby.";
                    }
                    _phrase552 = _phrase552.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase552), Config.Server_Response_Name, false, "ServerTools", false));
                }
                string[] _cords = SetLobby.Lobby_Position.Split(',');
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                Players.NoFlight.Add(_cInfo.entityId);
                TeleportDelay.TeleportQue(_cInfo, x, y, z);
                string _phrase553;
                if (!Phrases.Dict.TryGetValue(553, out _phrase553))
                {
                    _phrase553 = "{PlayerName} sending you to the lobby.";
                }
                _phrase553 = _phrase553.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase553), Config.Server_Response_Name, false, "ServerTools", false));
                int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].LastLobby = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                string _phrase554;
                if (!Phrases.Dict.TryGetValue(554, out _phrase554))
                {
                    _phrase554 = "{PlayerName} the lobby position is not set.";
                }
                _phrase554 = _phrase554.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase554), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void SendBack(ClientInfo _cInfo, string _playerName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                if (p.LobbyReturn != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    int x, y, z;
                    string[] _cords = SetLobby.Lobby_Position.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Lobby_Size * Lobby_Size)
                    {
                        string[] _returnCoords = p.LobbyReturn.Split(',');
                        int.TryParse(_returnCoords[0], out x);
                        int.TryParse(_returnCoords[1], out y);
                        int.TryParse(_returnCoords[2], out z);
                        Players.NoFlight.Add(_cInfo.entityId);
                        TeleportDelay.TeleportQue(_cInfo, x, y, z);
                        LobbyPlayers.Remove(_cInfo.entityId);
                        string _phrase555;
                        if (!Phrases.Dict.TryGetValue(555, out _phrase555))
                        {
                            _phrase555 = "{PlayerName} sending you back to your saved location.";
                        }
                        _phrase555 = _phrase555.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase555), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        string _phrase556;
                        if (!Phrases.Dict.TryGetValue(556, out _phrase556))
                        {
                            _phrase556 = "{PlayerName} you are outside the lobby. Get inside it and try again.";
                        }
                        _phrase556 = _phrase556.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase556), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
        }
    }
}
