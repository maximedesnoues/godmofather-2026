using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "MailData", menuName = "ScriptableObject/MailData", order = 0)]

public class MailData : ScriptableObject
{
    public enum MailType { Mom, Boss, Story }

    public MailType mailType;   
    public string title;
    public string content;

    public string leftButtonText;
    [ShowIf("mailType", MailType.Story)]
    public int leftButtonMoney;
    [ShowIf("mailType", MailType.Story)]
    public int leftButtonKarma;

    [ShowIf("mailType", MailType.Story)]
    public string rightButtonText;
    [ShowIf("mailType", MailType.Story)]
    public int rightButtonMoney;
    [ShowIf("mailType", MailType.Story)]
    public int rightButtonKarma;

    public MailData nextMailGood;
    [ShowIf("mailType", MailType.Story)]
    public MailData nextMailBad;

    public int nextMailDay;
}
