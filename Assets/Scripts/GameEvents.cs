using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{

    public static GameEvents instance;

    private void Awake()
    {
        instance = this;
    }

    public event Action onActionPreparedEvent;

    public void OnActionPreparedEvent()
    {
        if (onActionPreparedEvent != null)
            onActionPreparedEvent();
    }

    public event Action onSuccessfulPlayerEvent;

    public void OnSuccessfulPlayerEvent()
    {
        if (onSuccessfulPlayerEvent != null)
            onSuccessfulPlayerEvent();
    }

    public event Action onSuccessfulAllyEvent;

    public void OnSuccessfulAllyEvent()
    {
        if (onSuccessfulAllyEvent != null)
            onSuccessfulAllyEvent();
    }

    public event Action onSuccessfulEnemyEvent;

    public void OnSuccessfulEnemyEvent()
    {
        if (onSuccessfulEnemyEvent != null)
            onSuccessfulEnemyEvent();
    }

    public event Action onSuccessfulPogo;

    public void OnSuccessfulPogo()
    {
        if(onSuccessfulPogo != null)
            onSuccessfulPogo();
    }

    public event Action onFailedPogo;

    public void OnFailedPogo()
    {
        if(onFailedPogo != null)
            onFailedPogo();
    }

    public event Action onGenerationComplete;
    public void OnGenerationComplete()
    {
        if(onGenerationComplete != null)
            onGenerationComplete();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
