﻿using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool BlockIsEnabled = false, FallingTreeEnabled = false, Underground = false, Bikes = false;
        private static List<Entity> Entities = new List<Entity>();
        private static List<int> FallingTree = new List<int>();
        private static int _xMinCheck, _yMinCheck, _zMinCheck, _xMaxCheck, _yMaxCheck, _zMaxCheck;

        public static void EntityCheck()
        {
            World world = GameManager.Instance.World;
            Entities = world.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity _entity = Entities[i];
                if (_entity != null)
                {
                    if (!_entity.IsClientControlled())
                    {
                        string _name = EntityClass.list[_entity.entityClass].entityClassName;
                        if (BlockIsEnabled)
                        {
                            if (_name == "fallingBlock")
                            {
                                Vector3 _vec = _entity.position;
                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                EntityPlayer _douche = world.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, 10, false);
                                if (_douche == null)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0} @ {1} {2} {3}", _entity.entityId, (int)_vec.x, (int)_vec.y, (int)_vec.z));
                                }
                                else
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_douche.entityId);
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0} @ {1} {2} {3}. Closest player is {4}", _entity.entityId, (int)_vec.x, (int)_vec.y, (int)_vec.z, _cInfo.playerName));
                                }
                            }
                        }
                        if (FallingTreeEnabled)
                        {
                            if (_name == "fallingTree")
                            {
                                if (!FallingTree.Contains(_entity.entityId))
                                {
                                    FallingTree.Add(_entity.entityId);
                                }
                                else
                                {
                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                    FallingTree.Remove(_entity.entityId);
                                    Log.Out("[SERVERTOOLS] Entity cleanup: Removed falling tree");
                                }
                            }
                        }
                        if (Underground)
                        {
                            int y = (int)_entity.position.y;
                            if (y <= -60)
                            {
                                if (_name == "fallingBlock")
                                {
                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0} from underground", _entity.entityId));
                                }
                                else
                                {
                                    int x = (int)_entity.position.x;
                                    int z = (int)_entity.position.z;
                                    _entity.SetPosition(new Vector3(x, -1, z));
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Teleported entity id {0} to the surface @ {1} -1 {2}", _entity.entityId, x, z));
                                }
                            }
                        }
                        if (Bikes)
                        {
                            if (_name == "minibike")
                            {
                                Vector3 _vec = _entity.position;
                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                EntityPlayer _douche = world.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, 10, false);
                                if (_douche == null)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed minibike id {0}", _entity.entityId));
                                }
                                else
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_douche.entityId);
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed minibike id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ZombieCheck()
        {
            if (Players.Box.Count > 0)
            {
                for (int i = 0; i < Players.Box.Count; i++)
                {
                    string[] _box = Players.Box[i];
                    bool _remove;
                    if (bool.TryParse(_box[6], out _remove))
                    {
                        if (_remove)
                        {
                            Entities = GameManager.Instance.World.Entities.list;
                            for (int j = 0; j < Entities.Count; j++)
                            {
                                Entity _entity = Entities[j];
                                if (_entity != null)
                                {
                                    if (!_entity.IsClientControlled() && !_entity.IsDead())
                                    {
                                        EntityType _type = _entity.entityType;
                                        if (_type == EntityType.Zombie)
                                        {
                                            Vector3 _vec = _entity.position;
                                            int _X = (int)_entity.position.x;
                                            int _Y = (int)_entity.position.y;
                                            int _Z = (int)_entity.position.z;
                                            int xMin, yMin, zMin;
                                            string[] _corner1 = _box[0].Split(',');
                                            int.TryParse(_corner1[0], out xMin);
                                            int.TryParse(_corner1[1], out yMin);
                                            int.TryParse(_corner1[2], out zMin);
                                            int xMax, yMax, zMax;
                                            string[] _corner2 = _box[1].Split(',');
                                            int.TryParse(_corner2[0], out xMax);
                                            int.TryParse(_corner2[1], out yMax);
                                            int.TryParse(_corner2[2], out zMax);
                                            if (xMin >= 0 & xMax >= 0)
                                            {
                                                if (xMin < xMax)
                                                {
                                                    if (_X >= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X <= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_X <= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X >= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (xMin <= 0 & xMax <= 0)
                                            {
                                                if (xMin < xMax)
                                                {
                                                    if (_X >= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X <= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_X <= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X >= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (xMin <= 0 & xMax >= 0)
                                            {
                                                if (_X >= xMin)
                                                {
                                                    _xMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _xMinCheck = 0;
                                                }
                                                if (_X <= xMax)
                                                {
                                                    _xMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _xMaxCheck = 0;
                                                }
                                            }
                                            else if (xMin >= 0 & xMax <= 0)
                                            {
                                                if (_X <= xMin)
                                                {
                                                    _xMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _xMinCheck = 0;
                                                }
                                                if (_X >= xMax)
                                                {
                                                    _xMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _xMaxCheck = 0;
                                                }
                                            }

                                            if (yMin >= 0 & yMax >= 0)
                                            {
                                                if (yMin < yMax)
                                                {
                                                    if (_Y >= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y <= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_Y <= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y >= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (yMin <= 0 & yMax <= 0)
                                            {
                                                if (yMin < yMax)
                                                {
                                                    if (_Y >= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y <= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_Y <= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y >= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (yMin <= 0 & yMax >= 0)
                                            {
                                                if (_Y >= yMin)
                                                {
                                                    _yMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _yMinCheck = 0;
                                                }
                                                if (_Y <= yMax)
                                                {
                                                    _yMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _yMaxCheck = 0;
                                                }
                                            }
                                            else if (yMin >= 0 & yMax <= 0)
                                            {
                                                if (_Y <= yMin)
                                                {
                                                    _yMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _yMinCheck = 0;
                                                }
                                                if (_Y >= yMax)
                                                {
                                                    _yMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _yMaxCheck = 0;
                                                }
                                            }

                                            if (zMin >= 0 & zMax >= 0)
                                            {
                                                if (zMin < zMax)
                                                {
                                                    if (_Z >= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z <= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_Z <= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z >= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (zMin <= 0 & zMax <= 0)
                                            {
                                                if (zMin < zMax)
                                                {
                                                    if (_Z >= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z <= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (_Z <= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z >= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                            }
                                            else if (zMin <= 0 & zMax >= 0)
                                            {
                                                if (_Z >= zMin)
                                                {
                                                    _zMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _zMinCheck = 0;
                                                }
                                                if (_Z <= zMax)
                                                {
                                                    _zMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _zMaxCheck = 0;
                                                }
                                            }
                                            else if (zMin >= 0 & zMax <= 0)
                                            {
                                                if (_Z <= zMin)
                                                {
                                                    _zMinCheck = 1;
                                                }
                                                else
                                                {
                                                    _zMinCheck = 0;
                                                }
                                                if (_Z >= zMax)
                                                {
                                                    _zMaxCheck = 1;
                                                }
                                                else
                                                {
                                                    _zMaxCheck = 0;
                                                }
                                            }
                                            if (_xMinCheck == 1 & _yMinCheck == 1 & _zMinCheck == 1 & _xMaxCheck == 1 & _yMaxCheck == 1 & _zMaxCheck == 1)
                                            {
                                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed zombie from protected zone @ {0} {1} {2}", _X, _Y, _Z));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
