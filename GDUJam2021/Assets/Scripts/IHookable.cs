using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHookable 
{
    void Hook(Transform parent);
    void Throw(Vector2 direction);
    void SetPosition(Vector2 position);
    void Hold();
    Rigidbody2D GetRB();
    Vector2 GetPosition();
    //void Hold(Transform holdPosition);

}
