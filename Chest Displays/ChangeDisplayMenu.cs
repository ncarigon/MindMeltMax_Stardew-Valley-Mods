﻿using Chest_Displays.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SObject = StardewValley.Object;
using SUtils = StardewValley.Utility;

namespace Chest_Displays
{
    public class ChangeDisplayMenu : MenuWithInventory
    {
        private const int BaseIdInventory = 36000;
        private const int OkButtonId = 35999;
        private const int DisplaySlotId = 69420; //Heh. Funny number

        private readonly Chest editingChest;
        private ModData? data;
        private ClickableTextureComponent displaySlot;
        private bool finishedInitializing = false;
        private Rectangle entryBackgroundBounds;

        private IModHelper _helper => ModEntry.IHelper;
        private IMonitor _monitor => ModEntry.IMonitor;

        public ChangeDisplayMenu(Chest c) : base(new InventoryMenu.highlightThisItem(i => i is not null), true, menuOffsetHack: 64)
        {
            editingChest = c;
            data = c.modData.ContainsKey(_helper.ModRegistry.ModID) ? JsonConvert.DeserializeObject<ModData>(c.modData[_helper.ModRegistry.ModID]) : null;
            loadViewComponents();
            snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent() => currentlySnappedComponent = getComponentWithID(BaseIdInventory);

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => Game1.activeClickableMenu = new ChangeDisplayMenu(editingChest);

        public override void setCurrentlySnappedComponentTo(int id)
        {
            currentlySnappedComponent = getComponentWithID(id);
            if (currentlySnappedComponent == null)
            {
                _monitor.Log($"Couldn't snap to component with id : {id}, Snapping to default", LogLevel.Warn);
                snapToDefaultClickableComponent();
            }
            Game1.playSound("smallSelect");
        }

        public override void setUpForGamePadMode()
        {
            snapToDefaultClickableComponent();
            snapCursorToCurrentSnappedComponent();
        }

        public override void applyMovementKey(int direction)
        {
            if (currentlySnappedComponent == null) snapToDefaultClickableComponent();
            switch (direction)
            {
                case 0: //Up
                    if (currentlySnappedComponent!.upNeighborID < 0) goto default;
                    setCurrentlySnappedComponentTo(currentlySnappedComponent.upNeighborID);
                    snapCursorToCurrentSnappedComponent();
                    break;
                case 1: //Right
                    if (currentlySnappedComponent!.rightNeighborID < 0) goto default;
                    setCurrentlySnappedComponentTo(currentlySnappedComponent.rightNeighborID);
                    snapCursorToCurrentSnappedComponent();
                    break;
                case 2: //Down
                    if (currentlySnappedComponent!.downNeighborID < 0) goto default;
                    setCurrentlySnappedComponentTo(currentlySnappedComponent.downNeighborID);
                    snapCursorToCurrentSnappedComponent();
                    break;
                case 3: //Left
                    if (currentlySnappedComponent!.leftNeighborID < 0) goto default;
                    setCurrentlySnappedComponentTo(currentlySnappedComponent.leftNeighborID);
                    snapCursorToCurrentSnappedComponent();
                    break;
                default:
                    base.applyMovementKey(direction);
                    break;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true) => receiveRightClick(x, y, playSound);

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!finishedInitializing) return;
            
            if (inventory.isWithinBounds(x, y))
            {
                Item? item = inventory.getItemAt(x, y);
                if (item is null) return;
                displaySlot.item = item.getOne();
                updateModData(item);
            }
            else if (displaySlot.bounds.Contains(x, y) && displaySlot.item is not null)
            {
                editingChest.modData.Remove(_helper.ModRegistry.ModID);
                data = null;
                displaySlot.item = null;
            }
            else if (okButton.bounds.Contains(x, y)) exitThisMenu();
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredItem = null;
            inventory.hover(x, y, null);

            foreach (var slot in inventory.inventory)
                if (slot.containsPoint(x, y))
                    hoveredItem = inventory.actualInventory.ElementAtOrDefault(inventory.inventory.IndexOf(slot));

            if (displaySlot.containsPoint(x, y))
                hoveredItem = displaySlot.item;

            base.performHoverAction(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.6f);
            draw(b, false, false);

            //These three things just to have the little backpack icon... AGAIN!!!
            b.Draw(Game1.mouseCursors, new Vector2((xPositionOnScreen - 64), (yPositionOnScreen + height / 2 + 64 + 16)), new Rectangle?(new Rectangle(16, 368, 12, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2((xPositionOnScreen - 64), (yPositionOnScreen + height / 2 + 64 - 16)), new Rectangle?(new Rectangle(21, 368, 11, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2((xPositionOnScreen - 40), (yPositionOnScreen + height / 2 + 64 - 44)), new Rectangle?(new Rectangle(4, 372, 8, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            Game1.drawDialogueBox(entryBackgroundBounds.X, entryBackgroundBounds.Y, entryBackgroundBounds.Width, entryBackgroundBounds.Height, false, true);

            displaySlot.draw(b);
            if (displaySlot.item is not null)
            {
                var pos = new Vector2(displaySlot.bounds.X + (displaySlot.bounds.Width / 2) - 32, displaySlot.bounds.Y + (displaySlot.bounds.Height / 2) - 32);
                Debug.WriteLine($"{displaySlot.bounds.Center.X}:{displaySlot.bounds.Center.Y} - {pos.X}:{pos.Y}");
                displaySlot.item.drawInMenu(b, pos, .75f, 1f, Utils.GetDepthFromItemType(Utils.getItemType(displaySlot.item), (int)pos.X, (int)pos.Y));
            }

            if (hoveredItem is not null)
                drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);

            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
            if (!finishedInitializing) finishedInitializing = true;
        }

        private void loadViewComponents()
        {
            inventory.showGrayedOutSlots = true;

            entryBackgroundBounds = new Rectangle(inventory.xPositionOnScreen + (inventory.width / 2 - 142), inventory.yPositionOnScreen - 344, 344, 344);
            displaySlot = new ClickableTextureComponent(new Rectangle(entryBackgroundBounds.Center.X - 60, entryBackgroundBounds.Center.Y - 24, 120, 120), Game1.menuTexture, new Rectangle(0, 256, 60, 60), 2f);

            if (data != null)
                displaySlot.item = Utils.getItemFromName(data.Item, data.ItemType, data.ItemQuality, data.UpgradeLevel);

            displaySlot.myID = DisplaySlotId;
            okButton.myID = OkButtonId;

            const int rowLength = 12;
            int colLength = inventory.inventory.Count / rowLength;

            for (int c = 0; c < colLength; c++)
            {
                for (int r = 0; r < rowLength; r++)
                {
                    int index = r + (rowLength * c);
                    inventory.inventory[index].myID = BaseIdInventory + index;

                    if (c != 0) inventory.inventory[index].upNeighborID = BaseIdInventory + index - rowLength;
                    else inventory.inventory[index].upNeighborID = DisplaySlotId;
                    if (c != (colLength - 1)) inventory.inventory[index].downNeighborID = BaseIdInventory + index + rowLength;
                    else inventory.inventory[index].downNeighborID = -1;
                    if (r != 0) inventory.inventory[index].leftNeighborID = BaseIdInventory + index - 1;
                    else inventory.inventory[index].leftNeighborID = -1;
                    if (r != (rowLength - 1)) inventory.inventory[index].rightNeighborID = BaseIdInventory + index + 1;
                    else inventory.inventory[index].rightNeighborID = -1;

                    if (r == (rowLength - 1) && c == (colLength - 1))
                    {
                        inventory.inventory[index].rightNeighborID = okButton.myID;
                        okButton.leftNeighborID = inventory.inventory[index].myID;
                    }

                    if (c == 0 && r == (rowLength / 2))
                        displaySlot.downNeighborID = inventory.inventory[index].myID;
                }
            }

            allClickableComponents = new();
            allClickableComponents.AddRange(inventory.inventory);
            allClickableComponents.Add(okButton);
            allClickableComponents.Add(displaySlot);
        }

        private void updateModData(Item item)
        {
            int itemType = Utils.getItemType(item);
            data = new()
            {
                Item = item is Tool t ? t.BaseName : Utils.GetItemNameFromIndex(Utils.GetItemIndexInParentSheet(item, itemType), itemType),
                Color = item is ColoredObject co ? co.color.Value : null,
                ItemQuality = item is SObject o ? o.Quality : -1,
                ItemType = itemType,
                UpgradeLevel = item is Tool tu ? tu.UpgradeLevel : -1
            };
            editingChest.modData[_helper.ModRegistry.ModID] = JsonConvert.SerializeObject(data);
        }
    }
}
