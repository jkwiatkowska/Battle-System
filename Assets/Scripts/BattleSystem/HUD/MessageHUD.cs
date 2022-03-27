using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageHUD : MonoBehaviour
{
    [SerializeField] ObjectPool MessagePool;
    [SerializeField] float MessageDisplayTime;
    public static MessageHUD Instance;
    Queue<(GameObject, float)> Messages;

    void Awake()
    {
        Instance = this;
        Messages = new Queue<(GameObject, float)>();
    }

    void Update()
    {
        if (Messages.Count > 0 && Messages.Peek().Item2 < BattleSystem.Time)
        {
            var message = Messages.Dequeue().Item1;
            message.SetActive(false);
            MessagePool.ReturnToPool(message);
        }
    }

    public void DisplayMessage(string messageText, Color color)
    {
        var message = MessagePool.GetFromPool();

        if (message == null)
        {
            message = Messages.Dequeue().Item1;
        }

        Messages.Enqueue((message, BattleSystem.Time + MessageDisplayTime));

        color.a = 1;
        var popup = message.GetComponent<PopupText>();
        if (popup != null)
        {
            popup.Setup(messageText, color);
        }
        else
        {
            var text = message.GetComponentInChildren<Text>();

            text.text = messageText;
            
            text.color = color;

            if (text == null)
            {
                Debug.LogError($"The object {message.name} doesn't have a Text component.");
                return;
            }
        }

        message.transform.SetAsLastSibling();
        message.SetActive(true);
    }
}