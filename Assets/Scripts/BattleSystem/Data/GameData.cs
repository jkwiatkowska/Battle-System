using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
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
            "healing"
        };

        EntityResources = new Dictionary<string, Value>()
        {
            {
                "hp", 
                new Value()
                {
                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 1.0f, "hp")
                }
            },
            {
                "mp",
                new Value()
                {
                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 1.0f, "mp")
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
                            SkillIDs = new List<string>(),
                            ResourcesAffected = new List<string>(),
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
                            SkillIDs = new List<string>(),
                            ResourcesAffected = new List<string>(),
                            Flags = new List<string>()
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
                                    MessageString = "Bomb explosion in 3.",
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
                                    PayloadData = new PayloadData()
                                    {
                                        Flags = new Dictionary<string, bool>()
                                        {
                                            { "ignoreDef", false },
                                            { "canCrit", true }
                                        },
                                        PayloadValue = new Value()
                                        {
                                            new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 3.0f, "atk")
                                        },
                                        Categories = new List<string>()
                                        {
                                            "physical"
                                        },
                                        SuccessChance = 1.0f,
                                        ResourceAffected = "hp"
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
                            SkillIDs = new List<string>(),
                            ResourcesAffected = new List<string>(),
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
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Attacking selected and tagging.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 0.8f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "singleTargetAttack"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackAction",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.1f,
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 15),
                                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 0.8f, "atk")
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp",
                                Tag = new TagData()
                                {
                                    TagID = "tag",
                                    TagDuration = 100000.0f,
                                    TagLimit = 3
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
                "singleTargetAttackWithDrain",
                new SkillData()
                {
                    SkillID = "singleTargetAttackWithDrain",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "singleTargetAttackWithDrain",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Attacking selected + tagged.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "singleTargetAttackWithDrain",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "singleTargetAttackWithDrain"
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "singleTargetAttackActionSmallHP",
                            SkillID = "singleTargetAttackWithDrain",
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 150)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnValueBelow,
                                    ConditionValueType = ActionCondition.eConditionValueType.ResourceCurrent,
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
                            ResourceName = "hp",
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
                            SkillID = "singleTargetAttackWithDrain",
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 10),
                                    new ValueComponent(ValueComponent.eValueComponentType.CasterResourceCurrent, 1, "hp")
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            },
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
                            ActionID = "attackTaggedAction",
                            SkillID = "singleTargetAttackWithDrain",
                            ActionType = Action.eActionType.PayloadDirect,
                            Timestamp = 0.5f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            ActionTargets = ActionPayloadDirect.eDirectActionTargets.TaggedEntity,
                            EntityTag = "tag",
                            TargetLimit = 30,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 22)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 9.0f,
                    CasterState = global::SkillData.eCasterState.Any,
                    MovementCancelsSkill = false
                }
            },
            {
                "coneAttack",
                new SkillData()
                {
                    SkillID = "coneAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "singleTargetAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Cylinder/cone attack.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
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
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(3.0f, 90.0f, 5.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 70)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction2",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.2f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(3.0f, 180.0f, 7.0f),
                                    InnerDimensions = new Vector2(0.0f, 90.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction3",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.3f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(3.0f, 270.0f, 6.0f),
                                    InnerDimensions = new Vector2(0.0f, 180.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "coneAction4",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.4f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(3.0f, 360.0f, 6.0f),
                                    InnerDimensions = new Vector2(0.0f, 270.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
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
                    NeedsTarget = false,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 3.0f,
                    CasterState = global::SkillData.eCasterState.Grounded,
                    MovementCancelsSkill = true
                }
            },
            {
                "cylinderAttack",
                new SkillData()
                {
                    SkillID = "cylinderAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "cylinderAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Cylinder attack.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "coneAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "cylinderAttack"
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "cylinderAction",
                            SkillID = "cylinderAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.1f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(1.0f, 360.0f, 5.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 10)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "cylinderAction",
                            SkillID = "cylinderAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.8f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(2.0f, 360.0f, 5.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 10)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "cylinderAction",
                            SkillID = "cylinderAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 1.6f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(3.0f, 360.0f, 5.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 10)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "cylinderAction",
                            SkillID = "cylinderAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 2.1f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cylinder,
                                    Dimensions = new Vector3(4.0f, 360.0f, 5.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.CasterPosition,
                                        ForwardSource = TransformData.eForwardSource.CasterForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 10)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                    },
                    NeedsTarget = false,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 3.0f,
                    CasterState = global::SkillData.eCasterState.Grounded,
                    MovementCancelsSkill = true
                }
            },
            {
                "rectangleAttack",
                new SkillData()
                {
                    SkillID = "rectangleAttack",
                    SkillTimeline = new ActionTimeline()
                    {
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Box/frame attack.",
                            MessageColor = Color.white
                        },
                        new ActionCooldown()
                        {
                            ActionID = "cd",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.ApplyCooldown,
                            Timestamp = 0.0f,
                            Cooldown = 1.0f,
                            CooldownTarget = ActionCooldown.eCooldownTarget.Skill,
                            CooldownTargetName = "rectangleAttack"
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "rectangleAction",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.1f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cube,
                                    Dimensions = new Vector3(1.0f, 1.0f, 2.0f),
                                    InnerDimensions = new Vector2(0.0f, 0.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.TargetPosition,
                                        ForwardSource = TransformData.eForwardSource.TargetForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 70)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "rectangleAction2",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 0.8f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cube,
                                    Dimensions = new Vector3(3.0f, 3.0f, 3.0f),
                                    InnerDimensions = new Vector2(1.8f, 2.5f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.TargetPosition,
                                        ForwardSource = TransformData.eForwardSource.TargetForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 0.9f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "rectangleAction3",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 1.6f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cube,
                                    Dimensions = new Vector3(5.0f, 4.5f, 3.0f),
                                    InnerDimensions = new Vector2(3.5f, 3.5f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.TargetPosition,
                                        ForwardSource = TransformData.eForwardSource.TargetForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 0.7f,
                                ResourceAffected = "hp"
                            }
                        },
                        new ActionPayloadArea()
                        {
                            ActionID = "rectangleAction4",
                            SkillID = "rectangleAttack",
                            ActionType = Action.eActionType.PayloadArea,
                            Timestamp = 2.1f,
                            TargetPriority = ActionPayload.eTargetPriority.Random,
                            TargetLimit = 50,
                            Target = ActionPayload.eTarget.EnemyEntities,
                            AreasAffected = new List<ActionPayloadArea.Area>()
                            {
                                new ActionPayloadArea.Area()
                                {
                                    Shape = ActionPayloadArea.Area.eShape.Cube,
                                    Dimensions = new Vector3(7.0f, 5.5f, 3.0f),
                                    InnerDimensions = new Vector2(5.5f, 5.0f),
                                    AreaTransform = new TransformData()
                                    {
                                        PositionOrigin = TransformData.ePositionOrigin.TargetPosition,
                                        ForwardSource = TransformData.eForwardSource.TargetForward
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 50)
                                },
                                Categories = new List<string>()
                                {
                                    "physical"
                                },
                                SuccessChance = 0.6f,
                                ResourceAffected = "hp"
                            }
                        }
                    },
                    NeedsTarget = true,
                    PreferredTarget = global::SkillData.eTargetPreferrence.Enemy,
                    Range = 20.0f,
                    CasterState = global::SkillData.eCasterState.Any,
                    MovementCancelsSkill = false
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
                        PreChargeTimeline = new ActionTimeline()
                        {
                            new ActionMessage()
                            {
                                ActionID = "message",
                                SkillID = "chargedAttack",
                                ActionType = Action.eActionType.Message,
                                Timestamp = 0.0f,
                                MessageString = "Charging a skill.",
                                MessageColor = Color.blue
                            },
                        },
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
                            ResourceName = "mp",
                            Value = 0.5f,
                            Optional = false
                        },
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "chargedAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Skill charged fully.",
                            MessageColor = Color.white,
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.ActionResultValue, 1.0f, "mpCollect")
                                },
                                Categories = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 0.6f,
                                ResourceAffected = "hp"
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.ActionResultValue, 1.0f, "mpCollect")
                                },
                                Categories = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 0.6f,
                                ResourceAffected = "hp"
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
                        new ActionMessage()
                        {
                            ActionID = "message",
                            SkillID = "chargedAttack",
                            ActionType = Action.eActionType.Message,
                            Timestamp = 0.0f,
                            MessageString = "Skill not charged fully.",
                            MessageColor = Color.white,
                            ActionConditions = new List<ActionCondition>()
                            {
                                new ActionCondition()
                                {
                                    Condition = ActionCondition.eActionCondition.OnActionFail,
                                    ConditionTarget = "randomTargetAttackAction"
                                }
                            },
                        },
                        new ActionPayloadDirect()
                        {
                            ActionID = "nearTargetAttackAction",
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
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 100)
                                },
                                Categories = new List<string>()
                                {
                                    "magic"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
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
                    },
                    PreferredTarget = global::SkillData.eTargetPreferrence.None,
                    CasterState = global::SkillData.eCasterState.Grounded,
                    MovementCancelsSkill = false
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
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, -0.1f, "hp")
                                },
                                Categories = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
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
                            Target = ActionPayload.eTarget.AllEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, -0.2f, "hp")
                                },
                                Categories = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
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
                            Target = ActionPayload.eTarget.AllEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, -0.3f, "hp")
                                },
                                Categories = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
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
                            Target = ActionPayload.eTarget.AllEntities,
                            PayloadData = new PayloadData()
                            {
                                Flags = new Dictionary<string, bool>()
                                {
                                    { "ignoreDef", true },
                                    { "canCrit", false }
                                },
                                PayloadValue = new Value()
                                {
                                    new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, -0.4f, "hp")
                                },
                                Categories = new List<string>()
                                {
                                    "healing"
                                },
                                SuccessChance = 1.0f,
                                ResourceAffected = "hp"
                            },
                            ActionConditions = new List<ActionCondition>()
                        }
                    },
                    CasterState = global::SkillData.eCasterState.Any,
                    MovementCancelsSkill = false
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
                                            PayloadData = new PayloadData()
                                            {
                                                Flags = new Dictionary<string, bool>()
                                                {
                                                    { "ignoreDef", false },
                                                    { "canCrit", true }
                                                },
                                                PayloadValue = new Value()
                                                {
                                                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 1.0f, "atk")
                                                },
                                                Categories = new List<string>()
                                                {
                                                    "physical"
                                                },
                                                SuccessChance = 1.0f,
                                                ResourceAffected = "hp",
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
                                            PayloadData = new PayloadData()
                                            {
                                                Categories = new List<string>(),
                                                PayloadValue = new Value(),
                                                Flags = new Dictionary<string, bool>(),
                                                SuccessChance = 1.0f,
                                                Triggers = new List<TriggerData.eTrigger>()
                                                {
                                                    TriggerData.eTrigger.OnDeath
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
                                            PayloadData = new PayloadData()
                                            {
                                                Flags = new Dictionary<string, bool>()
                                                {
                                                    { "ignoreDef", false },
                                                    { "canCrit", true }
                                                },
                                                PayloadValue = new Value()
                                                {
                                                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, -1.0f, "atk")
                                                },
                                                Categories = new List<string>()
                                                {
                                                    "healing"
                                                },
                                                SuccessChance = 1.0f,
                                                ResourceAffected = "hp",
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
                                            PayloadData = new PayloadData()
                                            {
                                                Flags = new Dictionary<string, bool>()
                                                {
                                                    { "ignoreDef", false },
                                                    { "canCrit", true }
                                                },
                                                PayloadValue = new Value()
                                                {
                                                    new ValueComponent(ValueComponent.eValueComponentType.CasterAttribute, 1.0f, "atk")
                                                },
                                                Categories = new List<string>()
                                                {
                                                    "magic"
                                                },
                                                SuccessChance = 1.0f,
                                                ResourceAffected = "hp",
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
