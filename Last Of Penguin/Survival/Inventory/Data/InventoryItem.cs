using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class InventoryItem
{
    public ItemDatabase item; // 특정 아이템의 ScriptableObject 참조
    public int amount; // 수량

    public InventoryItem(ItemDatabase item, int amount)
    {
        this.item = item; 
        this.amount = amount;
    }

    //Ref 복사를 막기 위한 클론 제작 함수
    public InventoryItem Clone()
    {
        return new InventoryItem(this.item, this.amount);
    }

    // Stack이 가능한지 확인
    public bool CanStackWith(InventoryItem other)
    {
        if (other == null || item == null) return false;
        return item == other.item && item.canStack;
    }

    // 아이템 추가 함수 (스택 가능한 경우에만 사용)
    public int Add(int value)
    {
        int addable = Mathf.Min(value, item.maxStack - amount);
        amount += addable;
        
        // 스택 가능한 최대치를 넘지 않도록 보정
        if (amount > item.maxStack)
        {
            int overflow = amount - item.maxStack;
            amount = item.maxStack;
            return value - overflow; // 실제로 추가된 양 반환
        }
        return addable; // 실제로 추가된 양 반환
    }
}