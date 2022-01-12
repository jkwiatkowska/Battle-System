using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDDamageDisplay : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] GameObject DamageTextPrefabPlayer;
    [SerializeField] GameObject DamageTextPrefabEnemy;

    public static HUDDamageDisplay Instance;

    void Awake()
    {
        Instance = this;    
    }

    public void DisplayDamage(string textToDisplay, Entity target, bool isPlayer)
    {
        GameObject damageText = Instantiate((isPlayer ? DamageTextPrefabPlayer : DamageTextPrefabEnemy), transform);

        var text = damageText.GetComponentInChildren<Text>();
        text.text = textToDisplay;

        var position = target.transform.position;
        position.y += target.EntityData.Height;

        damageText.GetComponent<DamageText>().Setup(position, HUD, text);
    }
}
