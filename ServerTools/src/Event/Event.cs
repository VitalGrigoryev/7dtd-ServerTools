﻿using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Setup = false, Open = false, Name = false, Invite = false, Info = false, Spawn = false, Respawn = false, Complete = false, Invited = false, Cancel = false, Extend = false, Return = false;
        public static Dictionary<int, int> PlayersTeam = new Dictionary<int, int>();
        public static Dictionary<int, string> PlayersReturn = new Dictionary<int, string>();
        public static List<int> Players = new List<int>();
        public static List<int> SpawnList = new List<int>();
        public static List<string> Spawning = new List<string>();
        public static List<string> Respawning = new List<string>();
        public static string Admin = null;

        public static void CheckOpen()
        {
            if (!Invited)
            {
                Setup = false;
                Name = false;
                Invite = false;
                Info = false;
                Spawn = false;
                Respawn = false;
                Complete = false;
                Invited = false;
                Admin = null;
                PlayersTeam.Clear();
                Players.Clear();
                PlayersReturn.Clear();
                SpawnList.Clear();
                Spawning.Clear();
                Respawning.Clear();
                PersistentContainer.Instance.EventName = null;
                PersistentContainer.Instance.EventInvite = null;
                PersistentContainer.Instance.EventPlayerCount = 0;
                PersistentContainer.Instance.EventTeams = 0;
                PersistentContainer.Instance.EventTime = 0;
                PersistentContainer.Instance.EventSpawn = null;
                PersistentContainer.Instance.EventRespawn = null;
                PersistentContainer.Instance.Save();
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Admin);
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have taken too long to setup the event. The setup has been cleared.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void CheckOpen2()
        {
            if (!Open)
            {
                Setup = false;
                Name = false;
                Invite = false;
                Info = false;
                Spawn = false;
                Respawn = false;
                Complete = false;
                Invited = false;
                Admin = null;
                PlayersTeam.Clear();
                Players.Clear();
                PlayersReturn.Clear();
                SpawnList.Clear();
                Spawning.Clear();
                Respawning.Clear();
                PersistentContainer.Instance.EventName = null;
                PersistentContainer.Instance.EventInvite = null;
                PersistentContainer.Instance.EventPlayerCount = 0;
                PersistentContainer.Instance.EventTeams = 0;
                PersistentContainer.Instance.EventTime = 0;
                PersistentContainer.Instance.EventSpawn = null;
                PersistentContainer.Instance.EventRespawn = null;
                PersistentContainer.Instance.Save();
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Admin);
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} not enough players signed up for the event. The setup has been cleared.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (!Players.Contains(_cInfo.entityId))
            {
                Players.Add(_cInfo.entityId);
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _sposition = x + "," + y + "," + z;
                PlayersReturn.Add(_cInfo.entityId, _sposition);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have signed up for the event and your current location has been saved.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                int _playerCount = PersistentContainer.Instance.EventPlayerCount;
                if (Players.Count == _playerCount)
                {
                    Setup = false;
                    Name = false;
                    Invite = false;
                    Info = false;
                    Spawn = false;
                    Respawn = false;
                    Complete = false;
                    Invited = false;
                    int _teamCount = PersistentContainer.Instance.EventTeams;
                    for (int i = 0; i < Players.Count; i++)
                    {
                        int _playerEntId = Players[i];
                        ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                        if (_cInfo2 != null)
                        {
                            PlayersTeam.Add(_playerEntId, _teamCount);
                            string _spawnPos = Spawning[_teamCount - 1];
                            int _x, _y, _z;
                            string[] _cords = _spawnPos.Split(',');
                            int.TryParse(_cords[0], out _x);
                            int.TryParse(_cords[1], out _y);
                            int.TryParse(_cords[2], out _z);
                            _cInfo2.SendPackage(new NetPackageTeleportPlayer(new Vector3(_x, _y, _z), false));
                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are on team {2}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _teamCount), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been sent to your event spawn point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            _teamCount--;
                        }
                        if (_teamCount == 0)
                        {
                            _teamCount = PersistentContainer.Instance.EventTeams;
                        }
                    }
                    int _eventTime = PersistentContainer.Instance.EventTime * 60;
                    Timers._eventTime = _eventTime;
                    PersistentContainer.Instance.EventNameOld = PersistentContainer.Instance.EventName;
                    PersistentContainer.Instance.EventInviteOld = PersistentContainer.Instance.EventInvite;
                    PersistentContainer.Instance.EventPlayerCountOld = PersistentContainer.Instance.EventPlayerCount;
                    PersistentContainer.Instance.EventTeamsOld = PersistentContainer.Instance.EventTeams;
                    PersistentContainer.Instance.EventTimeOld = PersistentContainer.Instance.EventTime;
                    PersistentContainer.Instance.EventSpawnOld = PersistentContainer.Instance.EventSpawn;
                    PersistentContainer.Instance.EventRespawnOld = PersistentContainer.Instance.EventRespawn;
                    PersistentContainer.Instance.Save();
                    Open = true;
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} is full and has now started.", Config.Chat_Response_Color, PersistentContainer.Instance.EventName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} still has space for more players. Type /event.", Config.Chat_Response_Color, PersistentContainer.Instance.EventName), Config.Server_Response_Name, false, "ServerTools", true);
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of {2} have signed up.", Config.Chat_Response_Color, Players.Count, PersistentContainer.Instance.EventPlayerCount), Config.Server_Response_Name, false, "ServerTools", true);
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are already signed up for this event. It will start when enough players sign up.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Died(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int _team;
            PlayersTeam.TryGetValue(_player.entityId, out _team);
            string _spawnPos = Respawning[_team - 1];
            int _x, _y, _z;
            string[] _cords = _spawnPos.Split(',');
            int.TryParse(_cords[0], out _x);
            int.TryParse(_cords[1], out _y);
            int.TryParse(_cords[2], out _z);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_x, _y, _z), false));
            SpawnList.Remove(_cInfo.entityId);
        }

        public static void HalfTime()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                int _playerEntId = Players[i];
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event is at half time.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event is at half time.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void FiveMin()
        {
            Extend = true;
            for (int i = 0; i < Players.Count; i++)
            {
                int _playerEntId = Players[i];
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has five minutes remaining.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has five minutes remaining. If you need to extend the time remaining, type event extend. It will add 30 more minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void EndEvent()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                int _playerEntId = Players[i];
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                if (_cInfo != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player.IsSpawned())
                    {
                        string _position;
                        PlayersReturn.TryGetValue(_playerEntId, out _position);
                        int x, y, z;
                        string[] _cords = _position.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                        PlayersReturn.Remove(_playerEntId);
                        PlayersTeam.Remove(_playerEntId);
                        Players.Remove(_playerEntId);
                        SpawnList.Remove(_playerEntId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has ended. Thank you for playing.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_playerEntId);
                    string _steamId = _persistentPlayerData.PlayerId;
                    string _pos;
                    PlayersReturn.TryGetValue(_playerEntId, out _pos);
                    PersistentContainer.Instance.Players[_steamId, true].EventReturn = _pos;
                    PersistentContainer.Instance.Save();
                    PlayersReturn.Remove(_playerEntId);
                    PlayersTeam.Remove(_playerEntId);
                    Players.Remove(_playerEntId);
                    SpawnList.Remove(_playerEntId);
                }
            }
            if (Players.Count == 0)
            {
                Open = false;
                Players = null;
                SpawnList = null;
                Spawning = null;
                Respawning = null;
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the current event has ended and event players have been sent back to their return points.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                Admin = null;
                PlayersReturn.Clear();
                PlayersTeam.Clear();
                Players.Clear();
                SpawnList.Clear();
                PersistentContainer.Instance.EventName = null;
                PersistentContainer.Instance.EventInvite = null;
                PersistentContainer.Instance.EventPlayerCount = 0;
                PersistentContainer.Instance.EventTeams = 0;
                PersistentContainer.Instance.EventTime = 0;
                PersistentContainer.Instance.EventSpawn = null;
                PersistentContainer.Instance.EventRespawn = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
                if (_cInfo != null)
                {
                    Return = true;
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the current event has ended and event players have been sent back to their return points.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} not all players were spawned. Let them respawn and type event return in console.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void OfflineReturn(ClientInfo _cInfo)
        {
            string _pos = PersistentContainer.Instance.Players[_cInfo.playerId, true].EventReturn;
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
            PersistentContainer.Instance.Players[_cInfo.playerId, true].EventReturn = null;
            PersistentContainer.Instance.Save();
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event ended while you were offline. You have been sent to your return point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
        }
    }
}
