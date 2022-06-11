using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions
{
    public void run()
    {
        float horizontalInput;
        Vector2 _movement;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        _movement = new Vector2(horizontalInput, 0f);
    }
}
