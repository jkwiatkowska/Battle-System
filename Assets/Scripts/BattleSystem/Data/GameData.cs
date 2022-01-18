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
    public static Dictionary<string, List<string>> SkillGroups; // Cooldowns can be applied to multiple skills at once. 

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
                            Actions = new ActionTimeline()
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
                    RotateSpeed = 5.0f,
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
                            Actions = new ActionTimeline()
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
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionCooldownApplication()
                        {
                            ActionID = "cd",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldownApplication.eCooldownTarget.Skill,
                            CooldownTargetName = "singleTargetAttack"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackActionSmallHP",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 200)
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
                                    ConditionValueBoundary = 200,
                                    ConditionTarget = "hp"
                                }
                            }
                        },
                        new ActionCostCollection()
                        {
                            ActionID = "hpCollect",
                            Timestamp = 0.0f,
                            ValueType = ActionCostCollection.eCostValueType.CurrentMult,
                            DepletableName = "hp",
                            Value = 0.1f,
                            Optional = false,
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnActionFail,
                                    ConditionTarget = "singleTargetAttackActionSmallHP"
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
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 10),
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableCurrent, 1, "hp")
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
                                    ConditionTarget = "singleTargetAttackActionSmallHP"
                                }
                            }
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 15.0f
                }
            },
            {
                "coneAttack",
                new SkillData()
                {
                    SkillID = "coneAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionCooldownApplication()
                        {
                            ActionID = "cd",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldownApplication.eCooldownTarget.Skill,
                            CooldownTargetName = "coneAttack"
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.1f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Circle,
                                    Dimensions = new Vector2(2.0f, 90.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                    }
                                }
                            },
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 70)
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction2",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.8f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Circle,
                                    Dimensions = new Vector2(2.0f, 180.0f),
                                    InnerDimensions = new Vector2(0.0f, 90.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                    }
                                }
                            },
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 50)
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction3",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 1.6f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Circle,
                                    Dimensions = new Vector2(2.0f, 270.0f),
                                    InnerDimensions = new Vector2(0.0f, 180.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                    }
                                }
                            },
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 50)
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction4",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 2.4f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Circle,
                                    Dimensions = new Vector2(3.0f, 360.0f),
                                    InnerDimensions = new Vector2(0.0f, 270.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                    }
                                }
                            },
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", false },
                                    { "canCrit", true }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, 50)
                                },
                                Affinities = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                DepletableAffected = "hp"
                            }
                        }
                    }, 
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 3.0f
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
                        PreChargeTimeline = new ActionTimeline(),
                        ShowUI = true
                    },
                    SkillTimeline = new ActionTimeline()
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
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.ActionResultValue, 1.0f, "mpCollect")
                                },
                                Affinities = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 0.6f,
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
                            },
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "randomTargetAttackAction",
                            SkillID = "chargedAttack",
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 6,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadComponents = new List<PayloadData.PayloadComponent>()
                                {
                                    new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.ActionResultValue, 1.0f, "mpCollect")
                                },
                                Affinities = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 0.6f,
                                DepletableAffected = "hp"
                            },
                            Timestamp = 0.4f,
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
                            PayloadData = new PayloadData()
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
                    SkillTimeline = new ActionTimeline()
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
                            PayloadData = new PayloadData()
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
                            PayloadData = new PayloadData()
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
                            PayloadData = new PayloadData()
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
                            PayloadData = new PayloadData()
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
