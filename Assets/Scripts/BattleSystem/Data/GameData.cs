using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static List<string> Affinities;                      // Damage types and resistances can be calculated using these
    public static List<string> EntityDepletables;               // Values like hit points, mana, stamina, etc.
    public static List<string> EntityAttributes;                // Stats mainly used to determine outgoing and incoming damage
    public static List<string> PayloadFlags;                    // Flags to customise payload damage

    public static Dictionary<string, FactionData> FactionData;  // Define entity allegiance and relations
    public static string PlayerFaction;                         // Which of the factions the player is in
    public static Dictionary<string, EntityData> EntityData;
    public static Dictionary<string, SkillData> SkillData;

    public static void LoadData(string path)
    {

    }

    public static void LoadMockData()
    {
        Affinities = new List<string>()
        {
            "physical",
            "magic",
            "healing"
        };

        EntityDepletables = new List<string>()
        {
            "hp",
            "mp"
        };

        EntityAttributes = new List<string>()
        {
            "hp",
            "mp",
            "atk",
            "def",
            "critChance",
            "critDamage"
        };

        PayloadFlags = new List<string>()
        {
            "ignoreDef",
            "canCrit"
        };

        FactionData = new Dictionary<string, FactionData>
        {
            {
                "Player",
                new FactionData()
                {
                    FactionID = "Player",
                    FriendlyFactions = new List<string>(),
                    EnemyFactions = new List<string>()
                    {
                        "Dummy"
                    }
                }
            },
            {
                "Dummy",
                new FactionData()
                {
                    FactionID = "Dummy",
                    FriendlyFactions = new List<string>(),
                    EnemyFactions = new List<string>()
                }
            }
        };

        PlayerFaction = "Player";

        EntityData = new Dictionary<string, EntityData>
        {
            {
                "Player",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, float>()
                    {
                        { "hp", 600.0f },
                        { "mp", 500.0f },
                        { "atk", 100.0f },
                        { "def", 100.0f },
                        { "critChance", 0.1f },
                        { "critDamage", 0.5f },
                    },
                    IsTargetable = true,
                    Faction = "Player",
                    IsAI = false,
                    Radius = 0.05f,
                    Height = 1.0f,
                    LifeDepletables = new List<string>()
                    {
                        "hp"
                    },
                    Triggers = new List<TriggerData>()
                    {
                        new TriggerData()
                        {
                            Trigger = TriggerData.eTrigger.OnDeath,
                            Cooldown = 0,
                            Limit = 0,
                            Actions = new List<Action>()
                            {
                                new ActionDestroySelf()
                                {
                                    ActionID = "deathAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 2.0f,
                                }
                            },
                            SkillIDs = new List<string>(),
                            DepletablesAffected = new List<string>(),
                            Flags = new List<string>()
                        }
                    },
                    MovementSpeed = 4.0f,
                    RotateSpeed = 1.0f,
                    JumpHeight = 1.0f
                }
            },
            {
                "Dummy",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, float>()
                    {
                        { "hp", 500.0f },
                        { "mp", 500.0f },
                        { "atk", 100.0f },
                        { "def", 100.0f },
                        { "critChance", 0.1f },
                        { "critDamage", 0.5f },
                    },
                    IsTargetable = true,
                    Faction = "Dummy",
                    IsAI = false,
                    Radius = 0.5f,
                    Height = 1.0f,
                    LifeDepletables = new List<string>()
                    {
                        "hp"
                    },
                    Triggers = new List<TriggerData>()
                    {
                        new TriggerData()
                        {
                            Trigger = TriggerData.eTrigger.OnDeath,
                            Cooldown = 0,
                            Limit = 0,
                            Actions = new List<Action>()
                            {
                                new ActionDestroySelf()
                                {
                                    ActionID = "deathAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 2.0f,
                                }
                            },
                            SkillIDs = new List<string>(),
                            DepletablesAffected = new List<string>(),
                            Flags = new List<string>()
                        }
                    }
                }
            }
        };

        SkillData = new Dictionary<string, SkillData>()
        {
            {
                "singleTargetAttack",
                new SkillData()
                {
                    SkillID = "singleTargetAttack",
                    SkillTimeline = new List<Action>()
                    {
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackActionSmallMP",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 100)
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnValueBelow,
                                    ConditionValueType = ActionCondition.eConditionValueType.DepletableCurrent,
                                    ConditionValueBoundary = 100,
                                    ConditionTarget = "mp"
                                }
                            }
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackAction",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 10),
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableCurrent, 1, "mp")
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnActionFail,
                                    ConditionTarget = "singleTargetAttackActionSmallMP"
                                }
                            }
                        }
                    }
                }
            },
            {
                "chargedAttack",
                new SkillData()
                {
                    SkillID = "chargedAttack",
                    SkillChargeData = new SkillChargeData()
                    {
                        RequiredChargeTime = 0.5f,
                        FullChargeTime = 2.0f,
                        MovementCancelsCharge = true,
                        PreChargeTimeline = new List<Action>(),
                        ShowUI = true
                    },
                    SkillTimeline = new List<Action>()
                    {
                        new ActionCostCollection()
                        {
                            ActionID = "mpCollect",
                            Timestamp = 0.0f,
                            ActionConditions = new List<ActionCondition>(),
                            ValueType = ActionCostCollection.eCostValueType.CurrentMult,
                            DepletableName = "mp",
                            Value = 0.5f,
                            Optional = false
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "randomTargetAttackAction",
                            SkillID = "chargedAttack",
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 6,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.CasterAttribute, 1.5f, "atk")
                                },
                                Affinities = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 0.5f,
                                DepletableAffected = "hp"
                            },
                            Timestamp = 0.1f,
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnValueAbove,
                                    ConditionValueType = ActionCondition.eConditionValueType.ChargeRatio,
                                    ConditionValueBoundary = 1.0f
                                }
                            }
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackAction",
                            SkillID = "chargedAttack",
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetPriority = ActionPayload.eTargetPriority.Nearest,
                            TargetLimit = 2,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 100)
                                },
                                Affinities = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            Timestamp = 0.1f,
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnActionFail,
                                    ConditionTarget = "randomTargetAttackAction"
                                }
                            }
                        }
                    }
                }
            },
            {
                "healAll",
                new SkillData()
                {
                    SkillID = "healAll",
                    SkillTimeline = new List<Action>()
                    {
                        new ActionPayloadDirect()
                        {
                            ActionID = "healAllAction",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax, -0.1f, "hp")
                                },
                                Affinities = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>() 
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "healAllAction",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.3f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax, -0.2f, "hp")
                                },
                                Affinities = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "healAllAction",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.6f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax, -0.3f, "hp")
                                },
                                Affinities = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "healAllAction",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 1.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            Payload = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax, -0.4f, "hp")
                                },
                                Affinities = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                        }
                    }
                }
            }
        };
    }

    #region Editor
    public static void SaveData(string path)
    {

    }

    public static void UpdateSkillID(string oldID, string newID)
    {
        if (SkillData.ContainsKey(oldID))
        {
            SkillData.Add(newID, SkillData[oldID]);
            SkillData.Remove(oldID);

            foreach (var entity in EntityData)
            {
                if (entity.Value.IsAI)
                {
                    var aiEntity = entity.Value as AIEntityData;
                    if (aiEntity == null)
                    {
                        Debug.LogError($"Entity {entity.Key} expected to be an AIEntity.");
                    }

                    var entitySkills = aiEntity.Skills;
                    while (entitySkills.Contains(oldID))
                    {
                        entitySkills[entitySkills.IndexOf(oldID)] = newID;
                    }
                }
            }
        }
    }

    public static void DeleteSkill(string skillID)
    {
        if (SkillData.ContainsKey(skillID))
        {
            SkillData.Remove(skillID);

            foreach (var entity in EntityData)
            {
                if (entity.Value.IsAI)
                {
                    var aiEntity = entity.Value as AIEntityData;
                    if (aiEntity == null)
                    {
                        Debug.LogError($"Entity {entity.Key} expected to be an AIEntity.");
                    }

                    var entitySkills = aiEntity.Skills;
                    if (entitySkills.Contains(skillID))
                    {
                        entitySkills.RemoveAll(s => s == skillID);
                    }
                }
            }
        }
    }
    #endregion

    public static EntityData GetEntityData(string entityID)
    {
        if (EntityData.ContainsKey(entityID))
        {
            return EntityData[entityID];
        }
        else
        {
            Debug.LogError($"Entity ID {entityID} could not be found.");
            return null;
        }
    }

    public static SkillData GetSkillData(string skillID)
    {
        if (SkillData.ContainsKey(skillID))
        {
            return SkillData[skillID];
        }
        else
        {
            Debug.LogError($"Skill ID {skillID} could not be found.");
            return null;
        }
    }

    public static FactionData GetFactionData(string factionID)
    {
        if (FactionData.ContainsKey(factionID))
        {
            return FactionData[factionID];
        }
        else
        {
            Debug.LogError($"Faction ID {factionID} could not be found.");
            return null;
        }
    }

    public static List<Entity> GetAllEntities()
    {
        List<Entity> entities = new List<Entity>();

        return entities;
    }

    public static List<Entity> GetEntitiesInRange(Vector3 position, float range)
    {
        List<Entity> entities = new List<Entity>();

        return entities;
    }
}
