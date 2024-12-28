package com.nkstudio.plugin

import android.widget.Toast
import com.unity3d.player.UnityPlayer

class NativeToast
{
    fun MakeToast(message: String)
    {
        UnityPlayer.currentActivity.runOnUiThread{
            val toast = Toast.makeText(UnityPlayer.currentActivity, message,Toast.LENGTH_SHORT)
            toast.show()
        }
    }
}