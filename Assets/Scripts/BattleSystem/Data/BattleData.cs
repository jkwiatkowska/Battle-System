using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleData
{
    public static List<string> Categories;                                  // Damage types and resistances can be calculated using these.
    public static Dictionary<string, Value> EntityResources;                // Values like hit points, mana, stamina, etc and their max values based on entity attributes.
    public static List<string> EntityAttributes;                            // Stats mainly used to determine outgoing and incoming damage.
    public static List<string> PayloadFlags;                                // Flags to customise payload damage.

    public static Dictionary<string, FactionData> FactionData;              // Define entity allegiance and relations.
    public static string PlayerFaction;                                     // Which of the factions the player is in.
    public static Dictionary<string, EntityData> EntityData;
    public static Dictionary<string, SkillData> SkillData;
    public static Dictionary<string, List<string>> SkillGroups;             // Cooldowns and effects can apply to multiple skills at once.
    public static Dictionary<string, StatusEffectData> StatusEffectData;
    public static Dictionary<string, List<string>> StatusEffectGroups;      // Effects can be grouped together and affected all at once.

    public static void LoadData(string path)
    {

    }

    public static void LoadMockData()
    {
        Categories = new List<string>()
        {
            "physical",
            "magic",
            "healing",
            "neutral",
            "fire",
            "water"
        };

        EntityResources = new Dictionary<string, Value>()
        {
            {
                "hp", 
                new Value()
                {
                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, "hp")
                }
            },
            {
                "mp",
                new Value()
                {
                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, "mp")
                }
            }
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
                        "Dummy",
                        "Object"
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
            },
            {
                "Object",
                new FactionData()
                {
                    FactionID = "Object",
                    FriendlyFactions = new List<string>(),
                    EnemyFactions = new List<string>()
                    {
                        "Dummy",
                        "Player"
                    }
                }
            }
        };

        SkillGroups = new Dictionary<string, List<string>>()
        {
            { "elemental", new List<string>() { "fireAttack", "waterAttack"} }
        };

        StatusEffectGroups = new Dictionary<string, List<string>>();

        StatusEffectData = new Dictionary<string, StatusEffectData>()
        {
            {
                "burn",
                new StatusEffectData()
                {
                    StatusID = "burn",
                    MaxStacks = 1,
                    Duration = 5.0f,
                    Effects = new List<Effect>(),
                    OnStacksGained = new List<(PayloadData, int)>()
                    {
                        (new PayloadData()
                        {
                            Flags = new List<string>()
                            {
                                "ignoreDef"
                            },
                            PayloadValue = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                            },
                            Categories = new List<string>()
                            {
                                "fire"
                            },
                            SuccessChance = 1.0f,
                            ResourceAffected = "hp"
                        },
                        1)
                    },
                    OnInterval = new List<(PayloadData, float)>()
                    {
                        (new PayloadData()
                        {
                            Flags = new List<string>()
                            {
                                "ignoreDef"
                            },
                            PayloadValue = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 25)
                            },
                            Categories = new List<string>()
                            {
                                "fire"
                            },
                            SuccessChance = 1.0f,
                            ResourceAffected = "hp"
                        },
                        0.2f)
                    }
                }
            },
            {
                "waterImmune",
                new StatusEffectData()
                {
                    StatusID = "waterImmune",
                    MaxStacks = 1,
                    Duration = 15.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectImmunity()
                        {
                            PayloadFilter = Effect.ePayloadFilter.Category,
                            PayloadName = "water",
                            Limit = 3,
                            EndStatusOnEffectEnd = true,
                            StacksRequired = new Vector2Int(1, 1),
                            EffectType = Effect.eEffectType.Immunity
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "hpGuard",
                new StatusEffectData()
                {
                    StatusID = "hpGuard",
                    MaxStacks = 1,
                    Duration = 15.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectResourceGuard()
                        {
                            Limit = 0,
                            EndStatusOnEffectEnd = true,
                            StacksRequired = new Vector2Int(1, 1),
                            EffectType = Effect.eEffectType.ResourceGuard,
                            Resource = "hp",
                            MinValue = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 1.0f)
                            },
                            MaxValue = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, 0.2f, "hp")
                            },
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "skillLock",
                new StatusEffectData()
                {
                    StatusID = "skillLock",
                    MaxStacks = 1,
                    Duration = 5.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectLock()
                        {
                            EffectType = Effect.eEffectType.Lock,
                            LockType = EffectLock.eLockType.SkillsAll,
                            StacksRequired = new Vector2Int(1, 1)
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "jumpLock",
                new StatusEffectData()
                {
                    StatusID = "jumpLock",
                    MaxStacks = 1,
                    Duration = 5.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectLock()
                        {
                            EffectType = Effect.eEffectType.Lock,
                            LockType = EffectLock.eLockType.Jump,
                            StacksRequired = new Vector2Int(1, 1)
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "moveLock",
                new StatusEffectData()
                {
                    StatusID = "moveLock",
                    MaxStacks = 1,
                    Duration = 5.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectLock()
                        {
                            EffectType = Effect.eEffectType.Lock,
                            LockType = EffectLock.eLockType.Movement,
                            StacksRequired = new Vector2Int(1, 1)
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "shield",
                new StatusEffectData()
                {
                    StatusID = "shield",
                    MaxStacks = 1,
                    Duration = 15.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectShield()
                        {
                            Limit = 20,
                            EndStatusOnEffectEnd = true,
                            StacksRequired = new Vector2Int(1, 1),
                            EffectType = Effect.eEffectType.Shield,
                            ShieldResource = "shield",
                            ShieldedResource = "hp",
                            SetMaxShieldResource = true,
                            ShieldResourceToGrant = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 300)
                            },
                            RemoveShieldResourceOnEffectEnd = true,
                            MaxDamageAbsorbed = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 300)
                            },
                            DamageMultiplier = 1.0f,
                            CategoryMultipliers = new Dictionary<string, float>()
                            {
                                { "fire", 1.5f },
                                { "neutral", 0.5f }
                            }
                        }
                    },
                    OnStacksGained = new List<(PayloadData, int)>(),
                    OnInterval = new List<(PayloadData, float)>()
                }
            },
            {
                "neutralBuff",
                new StatusEffectData()
                {
                    StatusID = "neutralBuff",
                    MaxStacks = 3,
                    Duration = 7.0f,
                    Effects = new List<Effect>()
                    {
                        new EffectAttributeChange()
                        {
                            EffectType = Effect.eEffectType.AttributeChange,
                            Attribute = "atk",
                            Value = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 15.0f)
                            },
                            StacksRequired = new Vector2Int(1, 2),
                            PayloadTargetType = Effect.ePayloadFilter.All,
                            PayloadTarget = "neutral"
                        },
                        new EffectAttributeChange()
                        {
                            EffectType = Effect.eEffectType.AttributeChange,
                            Attribute = "atk",
                            Value = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 25.0f)
                            },
                            StacksRequired = new Vector2Int(2, 2),
                            PayloadTargetType = Effect.ePayloadFilter.Category,
                            PayloadTarget = "neutral"
                        },
                        new EffectAttributeChange()
                        {
                            EffectType = Effect.eEffectType.AttributeChange,
                            Attribute = "atk",
                            Value = new Value()
                            {
                                new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 70.0f)
                            },
                            StacksRequired = new Vector2Int(3, 3),
                            PayloadTargetType = Effect.ePayloadFilter.Category,
                            PayloadTarget = "neutral"
                        }
                    }
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
                    BaseAttributes = new Dictionary<string, Vector2>()
                    {
                        { "hp", new Vector2(600.0f, 5000.0f) },
                        { "mp", new Vector2(500.0f, 700.0f) },
                        { "atk", new Vector2(100.0f, 2000.0f) },
                        { "def", new Vector2(100.0f, 900.0f) },
                        { "critChance", new Vector2(0.2f, 0.35f) },
                        { "critDamage", new Vector2(0.5f, 1.5f) },
                    },
                    IsTargetable = true,
                    Faction = "Player",
                    IsAI = false,
                    Radius = 0.05f,
                    Height = 1.0f,
                    OriginHeight = 0.5f,
                    LifeResources = new List<string>()
                    {
                        "hp"
                    },
                    Triggers = new List<TriggerData>()
                    {

                    },
                    MovementSpeed = 4.0f,
                    RotateSpeed = 5.0f,
                    JumpHeight = 1.0f
                }
            },
            {
                "Bullet",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, Vector2>()
                    {
                        { "hp", new Vector2(600.0f, 5000.0f) },
                        { "mp", new Vector2(500.0f, 700.0f) },
                        { "atk", new Vector2(100.0f, 2000.0f) },
                        { "def", new Vector2(100.0f, 900.0f) },
                        { "critChance", new Vector2(0.2f, 0.35f) },
                        { "critDamage", new Vector2(0.5f, 1.5f) },
                    },
                    IsTargetable = false,
                    Faction = "Player",
                    IsAI = false,
                    Radius = 0.05f,
                    Height = 0.1f,
                    LifeResources = new List<string>(),
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
                                    ActionID = "destroySelfAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 0.01f,
                                }
                            },
                            Conditions = new List<TriggerData.TriggerCondition>()
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
                    BaseAttributes = new Dictionary<string, Vector2>()
                    {
                        { "hp", new Vector2(600.0f, 5000.0f) },
                        { "mp", new Vector2(500.0f, 700.0f) },
                        { "atk", new Vector2(100.0f, 2000.0f) },
                        { "def", new Vector2(100.0f, 900.0f) },
                        { "critChance", new Vector2(0.1f, 0.3f) },
                        { "critDamage", new Vector2(0.5f, 1.5f) },
                    },
                    IsTargetable = true,
                    Faction = "Dummy",
                    IsAI = false,
                    Radius = 0.25f,
                    Height = 1.0f,
                    OriginHeight = 0.5f,
                    LifeResources = new List<string>()
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
                            Conditions = new List<TriggerData.TriggerCondition>()
                        }
                    }
                }
            },
            {
                "Bomb",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, Vector2>()
                    {
                        { "hp", new Vector2(1.0f, 1.0f) },
                        { "mp", new Vector2(1.0f, 1.0f) },
                        { "atk", new Vector2(100.0f, 200.0f) },
                        { "def", new Vector2(0.0f, 0.0f) },
                        { "critChance", new Vector2(0.3f, 0.3f) },
                        { "critDamage", new Vector2(1.5f, 1.5f) },
                    },
                    IsTargetable = true,
                    Faction = "Object",
                    IsAI = false,
                    Radius = 0.35f,
                    Height = 0.7f,
                    OriginHeight = 0.45f,
                    LifeResources = new List<string>()
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
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 0.0f,
                                    MessageString = "Fire trigger. Bomb explosion in 3.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 0.5f,
                                    MessageString = "Bomb explosion in 2.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 1.0f,
                                    MessageString = "Bomb explosion in 1.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 1.5f,
                                    MessageString = "Boom.",
                                    MessageColor = Color.red
                                },
                                new ActionPayloadArea()
                                {
                                    ActionID = "explode",
                                    SkillID = "",
                                    ActionType = Action.eActionType.PayloadArea,
                                    Timestamp = 1.5f,
                                    TargetPriority = ActionPayload.eTargetPriority.Random,
                                    TargetLimit = 50,
                                    Target = ActionPayload.eTarget.AllEntities,
                                    AreasAffected = new List<ActionPayloadArea.Area>()
                                    {
                                        new ActionPayloadArea.Area()
                                        {
                                            Shape = ActionPayloadArea.Area.eShape.Sphere,
                                            Dimensions = new Vector3(2.5f, 360.0f, 1.0f),
                                            InnerDimensions = new Vector2(0.0f, 0.0f),
                                            AreaTransform = new TransformData()
                                            {
                                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                            }
                                        }
                                    },
                                    PayloadData = new List<PayloadData>()
                                    {
                                        new PayloadData()
                                        {
                                                Flags = new List<string>()
                                                {
                                                    "ignoreDef"
                                                },
                                                PayloadValue = new Value()
                                                {
                                                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 3.0f, "atk")
                                                },
                                                Categories = new List<string>()
                                                {
                                                    "fire"
                                                },
                                                SuccessChance = 1.0f,
                                                ResourceAffected = "hp"
                                            }
                                    }
                                },
                                new ActionDestroySelf()
                                {
                                    ActionID = "deathAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 2.0f,
                                }
                            },
                            Conditions = new List<TriggerData.TriggerCondition>()
                            {
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Category,
                                    StringValue = "fire",
                                    DesiredOutcome = true
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Skill,
                                    StringValue = "fireAttack",
                                    DesiredOutcome = true
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.SkillGroup,
                                    StringValue = "elemental",
                                    DesiredOutcome = true
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Action,
                                    StringValue = "fireAttackAction",
                                    DesiredOutcome = true
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.EntityResource,
                                    StringValue = "hp",
                                    FloatValue = 0.1f,
                                    DesiredOutcome = false
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.TriggerSourceResourceRatio,
                                    StringValue = "hp",
                                    FloatValue = 0.5f,
                                    DesiredOutcome = true
                                }
                            }
                        },
                        new TriggerData()
                        {
                            Trigger = TriggerData.eTrigger.OnDeath,
                            Cooldown = 0,
                            Limit = 0,
                            Actions = new ActionTimeline()
                            {
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 0.0f,
                                    MessageString = "Neutral trigger. Bomb explosion in 3.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 0.5f,
                                    MessageString = "Bomb explosion in 2.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 1.0f,
                                    MessageString = "Bomb explosion in 1.",
                                    MessageColor = Color.red
                                },
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 1.5f,
                                    MessageString = "Boom.",
                                    MessageColor = Color.red
                                },
                                new ActionPayloadArea()
                                {
                                    ActionID = "explode",
                                    SkillID = "",
                                    ActionType = Action.eActionType.PayloadArea,
                                    Timestamp = 1.5f,
                                    TargetPriority = ActionPayload.eTargetPriority.Random,
                                    TargetLimit = 50,
                                    Target = ActionPayload.eTarget.EnemyEntities,
                                    AreasAffected = new List<ActionPayloadArea.Area>()
                                    {
                                        new ActionPayloadArea.Area()
                                        {
                                            Shape = ActionPayloadArea.Area.eShape.Sphere,
                                            Dimensions = new Vector3(1.5f, 360.0f, 1.0f),
                                            InnerDimensions = new Vector2(0.0f, 0.0f),
                                            AreaTransform = new TransformData()
                                            {
                                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition
                                            }
                                        }
                                    },
                                PayloadData = new List<PayloadData>()
                                {
                                    new PayloadData()
                                    {
                                            Flags = new List<string>()
                                            {

                                            },
                                            PayloadValue = new Value()
                                            {
                                                new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, "atk")
                                            },
                                            Categories = new List<string>()
                                            {
                                                "physical"
                                            },
                                            SuccessChance = 1.0f,
                                            ResourceAffected = "hp"
                                        }
                                    }
                                },
                                new ActionDestroySelf()
                                {
                                    ActionID = "deathAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 2.0f,
                                }
                            },
                            Conditions = new List<TriggerData.TriggerCondition>()
                            {
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Category,
                                    StringValue = "fire",
                                    DesiredOutcome = false
                                },
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Category,
                                    StringValue = "water",
                                    DesiredOutcome = false
                                }
                            }
                        },
                        new TriggerData()
                        {
                            Trigger = TriggerData.eTrigger.OnDeath,
                            Cooldown = 0,
                            Limit = 0,
                            Actions = new ActionTimeline()
                            {
                                new ActionMessage()
                                {
                                    ActionID = "message",
                                    SkillID = "",
                                    ActionType = Action.eActionType.Message,
                                    Timestamp = 0.0f,
                                    MessageString = "Water trigger. Bomb extinguished.",
                                    MessageColor = Color.red
                                },
                                new ActionDestroySelf()
                                {
                                    ActionID = "deathAction",
                                    SkillID = "",
                                    ActionType = Action.eActionType.DestroySelf,
                                    Timestamp = 1.0f,
                                }
                            },
                            Conditions = new List<TriggerData.TriggerCondition>()
                            {
                                new TriggerData.TriggerCondition()
                                {
                                    ConditionType = TriggerData.TriggerCondition.eConditionType.Category,
                                    StringValue = "water",
                                    DesiredOutcome = true
                                }
                            }
                        }
                    }
                }
            }
        };
        SkillData = new Dictionary<string, SkillData>()
        {
            {
                "neutralAttack",
                new SkillData()
                {
                    SkillID = "neutralAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "neutralAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Neutral attack + buff.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "neutralAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.3f,
                            Cooldown = 0.8f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "neutralAttack"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "neutralAttackAction",
                            SkillID = "neutralAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.5f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new List<PayloadData>()
                            {
                                new PayloadData()
                                {
                                    Flags = new List<string>(),
                                    PayloadValue = new Value()
                                    {
                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 4.8f, "atk")
                                    },
                                    Categories = new List<string>()
                                    {
                                        "neutral"
                                    },
                                    SuccessChance = 1.0f,
                                    ResourceAffected = "hp",
                                    ApplyStatus = new List<(string StatusID, int Stacks)>()
                                    {
                                        ("hpGuard", 1)
                                    }
                                }
                            },
                            ActionConditions = new List<ActionCondition>(),
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "neutralAttackBuff",
                            SkillID = "neutralAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.5f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.Self,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.FriendlyEntities,
                            PayloadData = new List<PayloadData>()
                            {
                                new PayloadData()
                                {
                                    Flags = new List<string>(),
                                    PayloadValue = new Value(),
                                    Categories = new List<string>()
                                    {
                                        "buff",
                                    },
                                    SuccessChance = 1.0f,
                                    ResourceAffected = "",
                                    ApplyStatus = new List<(string StatusID, int Stacks)>()
                                    {
                                        ("neutralBuff", 1),
                                        //("skillLock", 1),
                                        //("jumpLock", 1),
                                        //("moveLock", 1)
                                    }
                                }
                            },
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.ActionSuccess,
                                    ConditionTarget = "neutralAttackAction"
                                }
                            }
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 9.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "fireAttack",
                new SkillData()
                {
                    SkillID = "fireAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "fireAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Fire attack + burn.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "fireAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.3f,
                            Cooldown = 0.8f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "fireAttack"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "fireAttackAction",
                            SkillID = "fireAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.5f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new List<PayloadData>()
                            {
                                new PayloadData()
                                {
                                    Flags = new List<string>()
                                    {
                                        "canCrit"
                                    },
                                    PayloadValue = new Value()
                                    {
                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 4.8f, "atk")
                                    },
                                    Categories = new List<string>()
                                    {
                                        "fire"
                                    },
                                    SuccessChance = 1.0f,
                                    ResourceAffected = "hp",
                                    ApplyStatus = new List<(string StatusID, int Stacks)>()
                                    {
                                        ("burn", 1)
                                    }
                                }
                            },
                            ActionConditions = new List<ActionCondition>()
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 9.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "waterAttack",
                new SkillData()
                {
                    SkillID = "waterAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "waterAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Water attack + extinguish.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "waterAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.3f,
                            Cooldown = 0.8f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "waterAttack"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "waterAttackAction",
                            SkillID = "waterAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.5f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                            TargetLimit = 1,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new List<PayloadData>()
                            {
                                new PayloadData()
                                {
                                    Flags = new List<string>()
                                    {
                                        "canCrit"
                                    },
                                    PayloadValue = new Value()
                                    {
                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 4.8f, "atk")
                                    },
                                    Categories = new List<string>()
                                    {
                                        "water"
                                    },
                                    SuccessChance = 1.0f,
                                    ResourceAffected = "hp",
                                    ApplyStatus = new List<(string StatusID, int Stacks)>()
                                    {
                                        (StatusID: "waterImmune", Stacks: 1),
                                        (StatusID: "shield", Stacks: 1),
                                    },
                                    ClearStatus = new List<(bool StatusGroup, string StatusID)>()
                                    {
                                        (false, "burn")
                                    }
                                }
                            },
                            ActionConditions = new List<ActionCondition>(),
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 9.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "healAll",
                new SkillData()
                {
                    SkillID = "healAll",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Healing all entities.",
                            MessageColor = Color.green
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "healAllAction",
                            SkillID = "healAll",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.0f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.AllEntities,
                            TargetLimit = 10000,
                            Target = ActionPayload.eTarget.AllEntities,
                            PayloadData = new List<PayloadData>()
                            {
                                new PayloadData()
                                {
                                    Flags = new List<string>()
                                    {
                                        "ignoreDef", "canCrit"
                                    },
                                    PayloadValue = new Value()
                                    {
                                        new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, -1.0f, "hp")
                                    },
                                    Categories = new List<string>()
                                    {
                                        "healing"
                                    },
                                    SuccessChance = 1.0f,
                                    ResourceAffected = "hp"
                                }
                            },
                            ActionConditions = new List<ActionCondition>()
                        }
                    }
                }
            },
            {
                "summonSkill",
                new SkillData()
                {
                    SkillID = "summonSkill",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "summonSkill",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Summoning bomb.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "summonSkill",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 0.8f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "summonSkill"
                        },
                        new ActionSummon()
                        {
                            ActionID = "summonAction",
                            SkillID = "summonSkill",
                            ActionType = Action.eActionType.SpawnEntity,
                            Timestamp = 0.4f,
                            EntityID = "Bomb",
                            SharedAttributes = new Dictionary<string, float>(),
                            SummonAtPosition = new TransformData()
                            {
                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                ForwardSource = TransformData.eForwardSource.CasterForward,
                                PositionOffset = new Vector3(0.0f, 0.0f, 1.0f)
                            },
                            SummonDuration = 20.0f,
                            SummonLimit = 3
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 9.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "projectileSkill",
                new SkillData()
                {
                    SkillID = "projectileSkill",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "projectileSkill",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 0.05f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "projectileSkill"
                        },
                        new ActionProjectile()
                        {
                            ActionID = "projectileAction",
                            SkillID = "projectileSkill",
                            ActionType = Action.eActionType.SpawnProjectile,
                            Timestamp = 0.01f,
                            EntityID = "Bullet",
                            SharedAttributes = new Dictionary<string, float>()
                            {
                                { "atk", 0.9f }
                            },
                            SummonAtPosition = new TransformData()
                            {
                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                ForwardSource = TransformData.eForwardSource.CasterForward,
                                PositionOffset = new Vector3(0.0f, 0.5f, 0.5f),
                                RandomForwardOffset = 0.0f
                            },
                            SummonDuration = 16.0f,
                            SummonLimit = 60,
                            InheritFaction = true,
                            ProjectileMovementMode = ActionProjectile.eProjectileMovementMode.Arched,
                            ProjectileTimeline = new List<ActionProjectile.ProjectileState>(),
                            Target = ActionProjectile.eTarget.Target,
                            ArchAngle = 60.0f,
                            Gravity = -5.0f,
                            OnEnemyHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions,
                                    Actions = new ActionTimeline()
                                    {
                                        new ActionPayloadDirect()
                                        {
                                            ActionID = "projectileAttackAction",
                                            SkillID = "projectileAttack",
                                            ActionType = Action.eActionType.PayloadDirect,
                                            Timestamp = 0.0f,
                                            TargetPriority = ActionPayload.eTargetPriority.Random,
                                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                                            TargetLimit = 1,
                                            Target = ActionPayload.eTarget.EnemyEntities,
                                            PayloadData = new List<PayloadData>()
                                            {
                                                new PayloadData()
                                                {
                                                    Flags = new List<string>()
                                                    {
                                                        "ignoreDef", "canCrit"
                                                    },
                                                    PayloadValue = new Value()
                                                    {
                                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, "atk")
                                                    },
                                                    Categories = new List<string>()
                                                    {
                                                        "physical"
                                                    },
                                                    SuccessChance = 1.0f,
                                                    ResourceAffected = "hp",
                                                }
                                            },
                                        },
                                        new ActionPayloadDirect()
                                        {
                                            ActionID = "deathTriggerAction",
                                            SkillID = "projectileAttack",
                                            ActionType = Action.eActionType.PayloadDirect,
                                            Timestamp = 0.0f,
                                            Target = ActionPayload.eTarget.FriendlyEntities,
                                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.Self,
                                            TargetLimit = 1,
                                            PayloadData = new List<PayloadData>()
                                            {
                                                new PayloadData()
                                                {
                                                    Categories = new List<string>(),
                                                    PayloadValue = new Value(),
                                                    Flags = new List<string>(),
                                                    Instakill = true,
                                                    SuccessChance = 1.0f
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            OnFriendHit = new List<ActionProjectile.OnCollisionReaction>(),
                            OnTerrainHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.SelfDestruct
                                }
                            }
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 19.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "projectileSkill2",
                new SkillData()
                {
                    SkillID = "projectileSkill2",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "projectileSkill2",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 0.05f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "projectileSkill"
                        },
                        new ActionProjectile()
                        {
                            ActionID = "projectileAction",
                            SkillID = "projectileSkill2",
                            ActionType = Action.eActionType.SpawnProjectile,
                            Timestamp = 0.01f,
                            EntityID = "Bullet",
                            SharedAttributes = new Dictionary<string, float>()
                            {
                                { "atk", 0.9f }
                            },
                            SummonAtPosition = new TransformData()
                            {
                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                ForwardSource = TransformData.eForwardSource.CasterForward,
                                PositionOffset = new Vector3(0.0f, 0.45f, 15.5f),
                                RandomForwardOffset = 10.0f
                            },
                            SummonDuration = 16.0f,
                            SummonLimit = 100,
                            InheritFaction = true,
                            ProjectileMovementMode = ActionProjectile.eProjectileMovementMode.Homing,
                            ProjectileTimeline = new List<ActionProjectile.ProjectileState>()
                            {
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.2f, 0.2f),
                                    RotationPerSecond = new Vector2(5.0f, 5.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 0.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.3f, 0.3f),
                                    RotationPerSecond = new Vector2(5.0f, 5.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 1.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.1f, 0.2f),
                                    RotationPerSecond = new Vector2(5.0f, 5.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 1.1f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.0f, 0.1f),
                                    RotationPerSecond = new Vector2(5.0f, 5.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 3.5f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(1.0f, 1.1f),
                                    RotationPerSecond = new Vector2(5.0f, 5.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 5.0f
                                },
                            },
                            Target = ActionProjectile.eTarget.Caster,
                            Gravity = 0.0f,
                            OnFriendHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions,
                                    Actions = new ActionTimeline()
                                    {
                                        new ActionPayloadDirect()
                                        {
                                            ActionID = "projectileHealAction",
                                            SkillID = "projectileSkill2",
                                            ActionType = Action.eActionType.PayloadDirect,
                                            Timestamp = 0.01f,
                                            TargetPriority = ActionPayload.eTargetPriority.Random,
                                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                                            TargetLimit = 1,
                                            Target = ActionPayload.eTarget.FriendlyEntities,
                                            PayloadData = new List<PayloadData>()
                                            {
                                                new PayloadData()
                                                {
                                                    Flags = new List<string>()
                                                    {
                                                        "ignoreDef", "canCrit"
                                                    },
                                                    PayloadValue = new Value()
                                                    {
                                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, -1.0f, "atk")
                                                    },
                                                    Categories = new List<string>()
                                                    {
                                                        "healing"
                                                    },
                                                    SuccessChance = 1.0f,
                                                    ResourceAffected = "hp",
                                                }
                                            },
                                        }
                                    }
                                }
                            },
                            OnEnemyHit = new List<ActionProjectile.OnCollisionReaction>(),
                            OnTerrainHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.SelfDestruct
                                }
                            }
                        }
                    },
                    NeedsTarget = false,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Friendly,
                    Range = 19.0f,
                    CasterState = global::SkillData.eCasterState.Any
                }
            },
            {
                "projectileSkill3",
                new SkillData()
                {
                    SkillID = "projectileSkill3",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "projectileSkill3",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 0.05f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "projectileSkill"
                        },
                        new ActionProjectile()
                        {
                            ActionID = "projectileSummonAction",
                            SkillID = "projectileSkill3",
                            ActionType = Action.eActionType.SpawnProjectile,
                            Timestamp = 0.01f,
                            EntityID = "Bullet",
                            SharedAttributes = new Dictionary<string, float>()
                            {
                                { "atk", 0.9f }
                            },
                            SummonAtPosition = new TransformData()
                            {
                                PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                ForwardSource = TransformData.eForwardSource.CasterForward,
                                PositionOffset = new Vector3(0.0f, 0.45f, 0.5f),
                                RandomForwardOffset = 0.0f
                            },
                            SummonDuration = 66.0f,
                            SummonLimit = 100,
                            InheritFaction = true,
                            ProjectileMovementMode = ActionProjectile.eProjectileMovementMode.Orbit,
                            Anchor = ActionProjectile.eAnchor.Caster,
                            ProjectileTimeline = new List<ActionProjectile.ProjectileState>()
                            {
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.2f, 0.2f),
                                    RotationPerSecond = new Vector2(-50.0f, -50.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 0.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.0f, 0.0f),
                                    RotationPerSecond = new Vector2(50.0f, 50.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 1.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.0f, 0.0f),
                                    RotationPerSecond = new Vector2(50.0f, 50.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 4.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.2f, 0.2f),
                                    RotationPerSecond = new Vector2(-50.0f, -50.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 5.0f
                                },
                                new ActionProjectile.ProjectileState()
                                {
                                    SpeedMultiplier = new Vector2(0.0f, 0.0f),
                                    RotationPerSecond = new Vector2(50.0f, 50.0f),
                                    RotationY = new Vector2(0.0f, 0.0f),
                                    Timestamp = 6.0f
                                },
                            },
                            Target = ActionProjectile.eTarget.Caster,
                            Gravity = 0.0f,
                            OnFriendHit = new List<ActionProjectile.OnCollisionReaction>(),
                            OnEnemyHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions,
                                    Actions = new ActionTimeline()
                                    {
                                        new ActionPayloadDirect()
                                        {
                                            ActionID = "projectileAttackAction",
                                            SkillID = "projectileSkill3",
                                            ActionType = Action.eActionType.PayloadDirect,
                                            Timestamp = 0.01f,
                                            TargetPriority = ActionPayload.eTargetPriority.Random,
                                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.SelectedEntity,
                                            TargetLimit = 1,
                                            Target = ActionPayload.eTarget.EnemyEntities,
                                            PayloadData = new List<PayloadData>()
                                            {
                                                new PayloadData()
                                                {
                                                    Flags = new List<string>()
                                                    {
                                                        "ignoreDef", "canCrit"
                                                    },
                                                    PayloadValue = new Value()
                                                    {
                                                        new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, "atk")
                                                    },
                                                    Categories = new List<string>()
                                                    {
                                                        "magic"
                                                    },
                                                    SuccessChance = 1.0f,
                                                    ResourceAffected = "hp",
                                                }
                                            },
                                        }
                                    }
                                }
                            },
                            OnTerrainHit = new List<ActionProjectile.OnCollisionReaction>()
                            {
                                new ActionProjectile.OnCollisionReaction()
                                {
                                    Reaction = ActionProjectile.OnCollisionReaction.eReactionType.SelfDestruct
                                }
                            }
                        },
                        new ActionLoopBack()
                        {
                            ActionID = "loopBackAction",
                            SkillID = "projectileSkill",
                            ActionType = Action.eActionType.LoopBack,
                            Timestamp = 0.2f,
                            GoToTimestamp = 0.01f,
                            Loops = 4
                        }
                    },
                    NeedsTarget = false,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 19.0f,
                    CasterState = global::SkillData.eCasterState.Any
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