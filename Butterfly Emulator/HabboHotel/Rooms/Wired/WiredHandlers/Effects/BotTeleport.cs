﻿using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Users.Inventory;
using Butterfly.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class BotTeleport : IWiredTrigger, IWiredEffect, IWiredCycleable
    {
        private UInt32 itemID;
        private Room room;
        private WiredHandler handler;
        private List<RoomItem> items;
        private string botname; 
        private UInt32 time;
        private UInt32 cycles;
        private Boolean disposed;
        private readonly Random rnd;

        public BotTeleport(UInt32 itemID, Room room, WiredHandler handler, string botname, List<RoomItem> items, UInt32 time)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.botname = botname;
            this.items = items;
            this.time = time;
            this.disposed = false;
            this.cycles = 0;
            this.rnd = new Random();
        }

        public String Botname
        {
            get
            {
                return botname;
            }
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public UInt32 Time
        {
            get
            {
                return time;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (disposed)
                return;

            cycles = 0;
            if (time == 0 && user != null)
            {
                DoAction();
            }
            else
            {
                handler.RequestCycle(this);
            }
        }

        public bool OnCycle()
        {
            if (disposed)
                return false;

            cycles++;
            if (cycles > time)
            {
                DoAction();
                return false;
            }

            return true;
        }

        private void DoAction()
        {
            if (!string.IsNullOrEmpty(botname))
            {
                List<RoomUser> botList = room.GetRoomUserManager().GetBots;
                foreach (RoomUser bot in botList)
                {
                    if (bot.GetUsername().ToLower().Equals(botname.ToLower()))
                    {
                        if (items.Count > 0)
                        {
                            //AvatarEffectsInventoryComponent.ExecuteEffect(4, bot.VirtualId, room);

                            RoomItem item = items[rnd.Next(0, items.Count - 1)];
                            if (item != null)
                            {
                                room.GetGameMap().TeleportToItem(bot, item);
                                //AvatarEffectsInventoryComponent.ExecuteEffect(0, bot.VirtualId, room);
                            }
                        }

                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            disposed = true;
            room = null;
            handler = null;
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void ResetTimer()
        {

        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = botname + ";" + time + ";false";
            string wired_to_item = "";
            if (items.Count > 0)
            {
                lock (items)
                {
                    foreach (var id in items)
                    {
                        wired_to_item += id.Id + ";";
                    }
                    if (wired_to_item.Length > 0)
                        wired_to_item = wired_to_item.Substring(0, wired_to_item.Length - 1);
                }
            }
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID + "', @data" + itemID + ", @to_item" + itemID + ", @original_location" + itemID + ")");
            wiredInserts.AddParameter("data" + itemID, wired_data);
            wiredInserts.AddParameter("to_item" + itemID, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID, wired_original_location);
        }
    }
}
