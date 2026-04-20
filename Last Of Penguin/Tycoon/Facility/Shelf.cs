using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : Facility
{
    public bool HasItemOnShelf()
    {
        return currentOnItem != null; // currentItemOnShelf가 선반에 있는 아이템을 가리킨다고 가정
    }
}
