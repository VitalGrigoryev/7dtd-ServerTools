﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false, Set_Home2_Enabled = false, Set_Home2_Donor_Only = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void SetHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool Found = false;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            int _claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize) / 2;
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerData(_cInfo.playerId);
            List<Vector3i> _blocks = _persistentPlayerData.LPBlocks;
            for (int i = 0; i < _blocks.Count; i++)
            {
                Vector3i _vec3i = _blocks[i];
                if ((_vec3i.x - x) * (_vec3i.x - x) + (_vec3i.z - z) * (_vec3i.z - z) <= _claimSize * _claimSize)
                {
                    Found = true;
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = _sposition;
                    PersistentContainer.Instance.Save();
                    string _phrase10;
                    if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                    {
                        _phrase10 = "{PlayerName} your home has been saved.";
                    }
                    _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase10), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase10), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            if (!Found)
            {
                string _phrase817;
                if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                {
                    _phrase817 = "{PlayerName} you are not inside your own claimed space. You can not save this as your home.";
                }
                _phrase817 = _phrase817.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }       

        public static void TeleHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition == null)
            {
                string _phrase11;
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = "{PlayerName} you do not have a home saved.";
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int currentCoins = 0;
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
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        Home(_cInfo, p.HomePosition, _announce);
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            Home(_cInfo, p.HomePosition, _announce);
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
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
                                            Home(_cInfo, p.HomePosition, _announce);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    Home(_cInfo, p.HomePosition, _announce);
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
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

        private static void Home(ClientInfo _cInfo, string _home, bool _announce)
        {
            int x, y, z;
            string[] _cords = _home.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition != null)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void SetHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool Found = false;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            int _claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize) / 2;
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerData(_cInfo.playerId);
            List<Vector3i> _blocks = _persistentPlayerData.LPBlocks;
            for (int i = 0; i < _blocks.Count; i++)
            {
                Vector3i _vec3i = _blocks[i];
                if ((_vec3i.x - x) * (_vec3i.x - x) + (_vec3i.z - z) * (_vec3i.z - z) <= _claimSize * _claimSize)
                {
                    Found = true;
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = _sposition;
                    PersistentContainer.Instance.Save();
                    string _phrase607;
                    if (!Phrases.Dict.TryGetValue(607, out _phrase607))
                    {
                        _phrase607 = "{PlayerName} your home2 has been saved.";
                    }
                    _phrase607 = _phrase607.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase607), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase607), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            if (!Found)
            {
                string _phrase817;
                if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                {
                    _phrase817 = "{PlayerName} you are not inside your own claimed space. You can not save this as your home.";
                }
                _phrase817 = _phrase817.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void TeleHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition2 == null)
            {
                string _phrase608;
                if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = "{PlayerName} you do not have a home2 saved.";
                }
                _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int currentCoins = 0;
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
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        Home2(_cInfo, p.HomePosition2, _announce);
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            Home2(_cInfo, p.HomePosition2, _announce);
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
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
                                            Home2(_cInfo, p.HomePosition2, _announce);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    Home2(_cInfo, p.HomePosition2, _announce);
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
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

        private static void Home2(ClientInfo _cInfo, string _home2, bool _announce)
        {
            int x, y, z;
            string[] _cords = _home2.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition2 != null)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} deleted home2.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deleted home2.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} you have no home2 to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no home2 to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void FTeleHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition == null)
            {
                string _phrase11;
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = "{PlayerName} you do not have a home saved.";
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int currentCoins = 0;
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
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        Home(_cInfo, p.HomePosition, _announce);
                        FriendInvite(_cInfo, _player.position, p.HomePosition);
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            Home(_cInfo, p.HomePosition, _announce);
                            FriendInvite(_cInfo, _player.position, p.HomePosition);
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
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
                                            Home(_cInfo, p.HomePosition, _announce);
                                            FriendInvite(_cInfo, _player.position, p.HomePosition);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase815;
                                            if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                            {
                                                _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase815 = _phrase815.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    Home(_cInfo, p.HomePosition, _announce);
                                    FriendInvite(_cInfo, _player.position, p.HomePosition);
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void FTeleHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition2 == null)
            {
                string _phrase608;
                if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = "{PlayerName} you do not have a home2 saved.";
                }
                _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int currentCoins = 0;
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
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        Home2(_cInfo, p.HomePosition2, _announce);
                        FriendInvite(_cInfo, _player.position, p.HomePosition2);
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            Home2(_cInfo, p.HomePosition2, _announce);
                            FriendInvite(_cInfo, _player.position, p.HomePosition2);
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
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
                                            Home2(_cInfo, p.HomePosition2, _announce);
                                            FriendInvite(_cInfo, _player.position, p.HomePosition2);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase815;
                                            if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                            {
                                                _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                            _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    Home2(_cInfo, p.HomePosition2, _announce);
                                    FriendInvite(_cInfo, _player.position, p.HomePosition2);
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                EntityPlayer _player2 = world.Players.dict[_cInfo2.entityId];
                if (_player2 != null)
                {
                    if (_player.IsFriendsWith(_player2))
                    {
                        if ((x - _player2.position.x) * (x - _player2.position.x) + (z - _player2.position.z) * (z - _player2.position.z) <= 10 * 10)
                        {
                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your friend {2} has invited you to their saved home. Type /go to accept.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Invited your friend {1} to your saved home.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            Invite.Add(_cInfo2.entityId, DateTime.Now);
                            FriendPosition.Add(_cInfo2.entityId, _destination);
                        }
                    }
                }
            }
        }

        public static void FriendHome(ClientInfo _cInfo)
        {
            DateTime _dt;
            Invite.TryGetValue(_cInfo.entityId, out _dt);
            {
                TimeSpan varTime = DateTime.Now - _dt;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed <= 2)
                {
                    string _pos;
                    FriendPosition.TryGetValue(_cInfo.entityId, out _pos);
                    {
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        Players.NoFlight.Add(_cInfo.entityId);
                        TeleportDelay.TeleportQue(_cInfo, x, y, z);
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} sending you to your friend.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have run out of time to accept your friends invitation.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}