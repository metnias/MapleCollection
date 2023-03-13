using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using RWCustom;
using Menu;

namespace MapleCollection
{
    public static class TutorialPatch
    {
        public static void SubPatch()
        {
            //On.OverseerTutorialBehavior.TutorialText += new On.OverseerTutorialBehavior.hook_TutorialText(TextPatch);
            //On.OverseerTutorialBehavior.PickupObjectInputInstructionController.Update += new On.OverseerTutorialBehavior.PickupObjectInputInstructionController.hook_Update(PickupUpdatePatch);
            //On.RoomSpecificScript.SU_A23FirstCycleMessage.Update += new On.RoomSpecificScript.SU_A23FirstCycleMessage.hook_Update(SU_A23Patch);
            On.Menu.ControlMap.ctor += new On.Menu.ControlMap.hook_ctor(ControlMapPatch);
            //On.RoomSpecificScript.LF_A03.Update += new On.RoomSpecificScript.LF_A03.hook_Update(LF_A03Patch);
        }

        /*
        public static void GenDictionary()
        {
            swapDict = new Dictionary<string, int>
            {
                { "You are hungry, find food", 1 },
                { "Three is enough to hibernate", 2 },
                { "Additional food (above three) is kept for later", 3 },
                { "You are full", 4 }
            };
        }

        public static Dictionary<string, int> swapDict;

        public static void TextPatch(On.OverseerTutorialBehavior.orig_TutorialText orig, OverseerTutorialBehavior instance, string text, int wait, int time, bool hideHud)
        {
            if (swapDict == null) { GenDictionary(); }
            if (isSporecat && swapDict.TryGetValue(text, out int trigger))
            {
                switch (trigger)
                {
                    case 1:
                        orig.Invoke(instance, SporeMod.Translate("You are too young and weak to throw a metal rebar"), 10, 160, true);
                        orig.Invoke(instance, SporeMod.Translate("Have to create an opening"), 10, 200, true);
                        return;

                    case 2:
                        orig.Invoke(instance, SporeMod.Translate("You have to keep up with"), 10, 120, true);
                        return;

                    case 3:
                        orig.Invoke(instance, SporeMod.Translate("You will now last a bit longer"), 10, 120, true);
                        return;

                    case 4:
                        if (instance.player.slugcatStats.name == SlugcatStats.Name.Yellow) { return; }
                        break;
                }
            }
            orig.Invoke(instance, text, wait, time, hideHud);
        }

        public static bool extraTuto = true;

        public static void PickupUpdatePatch(On.OverseerTutorialBehavior.PickupObjectInputInstructionController.orig_Update orig, OverseerTutorialBehavior.PickupObjectInputInstructionController instance)
        {
            if (isSporecat && instance.overseer.AI.communication != null && instance.overseer.AI.communication.inputInstruction != null
                && !instance.overseer.AI.communication.inputInstruction.slatedForDeletetion && instance.overseer.AI.communication.inputInstruction is PickupObjectInstruction)
            {
                if (instance.room.abstractRoom.name == "SU_A23")
                {
                    if (!instance.textShown)
                    {
                        instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("You can sharpen your spear by double tap pick up button while holding rock and spear"), 0, 300, true, true);
                        instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("Sharpened spear will last longer before it gets stuck in creatures"), 20, 240, true, true);
                        instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("Then, hold up and pick up to pull out the spear from them"), 20, 240, true, true);
                        instance.textShown = true;
                        extraTuto = false;
                    }
                }
                if (!extraTuto && instance.room.abstractRoom.name == "SU_A25")
                {
                    instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("You can only stay in close combat for so long"), 0, 300, true, true);
                    instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("Once enervated, retreat and recompose"), 20, 240, true, true);
                    extraTuto = true;
                }
            }
            orig.Invoke(instance);
        }

        public static void SU_A23Patch(On.RoomSpecificScript.SU_A23FirstCycleMessage.orig_Update orig, RoomSpecificScript.SU_A23FirstCycleMessage instance, bool eu)
        {
            if (isSporecat && instance.room.game.session is StoryGameSession && !(instance.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.GoExploreMessage
                && instance.room.game.Players.Count > 0 && instance.room.game.Players[0].realizedCreature != null
                && instance.room.game.Players[0].realizedCreature.room == instance.room)
            {
                (instance.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.GoExploreMessage = true;
                instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("New adventure begins"), 20, 160, true, true);
                if (instance.room.game.cameras[0].hud.textPrompt.subregionTracker != null)
                {
                    instance.room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
                }
                instance.Destroy();
            }
            orig.Invoke(instance, eu);
        }

        public static void LF_A03Patch(On.RoomSpecificScript.LF_A03.orig_Update orig, RoomSpecificScript.LF_A03 instance, bool eu)
        {
            if (!isSporecat) { orig.Invoke(instance, eu); return; }
            Player player = null;
            if (instance.room.game.Players.Count > 0 && instance.room.game.Players[0].realizedCreature != null)
            {
                player = (instance.room.game.Players[0].realizedCreature as Player);
            }
            if (player != null && player.room == instance.room && instance.room.game.cameras[0].hud != null
                && instance.room.game.cameras[0].hud.textPrompt.messages.Count < 1)
            {
                if (instance.message > 2)
                {
                    instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("Larger prey needs to be incapacitated first"), 0, 160, true, true);
                    instance.room.game.cameras[0].hud.textPrompt.AddMessage(SporeMod.Translate("Then embark the journey anew"), 120, 160, true, true);
                    instance.room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial++;
                    Debug.Log("Hunter Tutorial Showed w/ Lancer Player: " + instance.room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial + " times");
                    instance.Destroy();
                    return;
                }
            }

            orig.Invoke(instance, eu);
        }*/

        public static void ControlMapPatch(On.Menu.ControlMap.orig_ctor orig, ControlMap map, Menu.Menu menu, MenuObject owner, Vector2 pos, Options.ControlSetup.Preset preset, bool showPickupInstructions)
        {
            orig.Invoke(map, menu, owner, pos, preset, showPickupInstructions);
            if (ModifyWorld.mapleworld == MapleEnums.MapleSlug.Not) return;
            if (showPickupInstructions)
            {
                string text = string.Empty;
                switch (ModifyWorld.mapleworld)
                {
                    case MapleEnums.MapleSlug.SlugSpore: //SporeCat
                        text = menu.Translate("Sporecat interactions:") + Environment.NewLine + Environment.NewLine;
                        text += "- " + menu.Translate("Sporecat's diet is exclusively insectivore, regardless of the prey's size") + Environment.NewLine;
                        text += "- " + menu.Translate("Hold UP and press PICK UP to grab a Puffball from the tail") + Environment.NewLine;
                        text += "- " + menu.Translate("Hold DOWN and PICK UP for charged explosion") + Environment.NewLine;
                        text += "- " + menu.Translate("However, using too many Puffballs costs hunger");
                        break;

                    case MapleEnums.MapleSlug.SlugSugar: //SugarCat
                        text = menu.Translate("Sugarcat interactions:") + Environment.NewLine + Environment.NewLine;
                        //text += "- " + menu.Translate("Hold UP and press PICK UP to grab a Puffball from the tail") + Environment.NewLine;
                        //text += "- " + menu.Translate("Hold DOWN and PICK UP for charged explosion") + Environment.NewLine;
                        //text += "- " + menu.Translate("Sporecat's diet is exclusively insectivore") + Environment.NewLine;
                        //text += "- " + menu.Translate("However, using too many Puffballs costs hunger");
                        break;

                    case MapleEnums.MapleSlug.SlugKnight: //DragonKnight
                        break;
                }
                Vector2 position = map.pickupButtonInstructions.pos;
                map.RemoveSubObject(map.pickupButtonInstructions);
                map.pickupButtonInstructions = new MenuLabel(menu, map, text, position, new Vector2(100f, 20f), false);
                map.pickupButtonInstructions.label.alignment = FLabelAlignment.Left;
                map.subObjects.Add(map.pickupButtonInstructions);
            }
        }
    }
}