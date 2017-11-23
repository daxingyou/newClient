﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class MJEntity : MonoBehaviour {
    public bool isCardUp = false;//控制OnMouseDown启用特殊功能
    private bool isSelect = false;
    public bool Select { get { return isSelect; } }
    private bool isEnable = true;
    private static int id = 0;
    private int cardId;
    public int CardId { get { return cardId; } }

    public int Card
    {
        get
        {
            int card = cardId / 100000;
            return card;
        }

        set
        {
            id++;
            cardId = value * 100000 + id;
        }
    }

    public bool IsMine()
    {
        MJPlayer player = this.GetComponentInParent<MJPlayer>();
        return (null != player && player.index == 0);
    }

    public bool IsHandCard()
    {
        HandCardLayout layout = this.GetComponentInParent<HandCardLayout>();
        return (null != layout);
    }
    //  麻将上下可选择  TODO
    public delegate void EventHandler(MJEntity card);
    public event EventHandler reSetPoisiton;
    public event EventHandler onSendMessage;

    void OnMouseDown()                                //按下
    {
        if (Game.MJMgr.HangUp)
        {
            return;
        }
        if (!(IsMine() && IsHandCard()))
        {
            return;
        }

        if (isCardUp)
        {
            SetSelect(!isSelect);
        }  
        else if (MJUtils.DropCard())
        {   
            if (isSelect == false)
            {
                isSelect = true;
                this.transform.localPosition = transform.localPosition;
            }
            else
            {
                if (onSendMessage != null)     //发送消息
                {
                    onSendMessage(this.GetComponent<MJEntity>());
                    OnClickDrop();
                }
            }
        }
    }

    private void OnMouseUp()      //按下之后的弹起来
    {
        if ( !isEnable)
        {
            return;
        }
        if (reSetPoisiton != null)
        {
            reSetPoisiton(this.GetComponent<MJEntity>());
        }
    }

    public void SetSelect(bool isSelect)
    {
        this.isSelect = isSelect;
        if (isSelect)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0.015f);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        }
    }

    public void SetEnable(bool enable)
    {
        this.isEnable = enable;

        MaterialManager cardMaterial = GameObject.FindObjectOfType<MaterialManager>();
        if(enable)
        {
            GetComponent<Renderer>().material = cardMaterial.myCardMatOn;
        }
        else
        {
            GetComponent<Renderer>().material = cardMaterial.myCardMatOff;
        }
    }

    public void OnClickDrop()
    {
        if (RoomMgr.actionNotify.tingList.Count <= 0 || RoomMgr.IsTingDropCard(Card))
        {
            Game.MJMgr.MyDropMJEntity = this;

            //Game.MJMgr.MyPlayer.DropCard(Card);
            Game.SocketGame.DoDropCard(Card);
            Game.MaterialManager.TurnOnHandCard();

            EventDispatcher.DispatchEvent(MessageCommand.MJ_UpdatePlayPage);
        }
    }
}
