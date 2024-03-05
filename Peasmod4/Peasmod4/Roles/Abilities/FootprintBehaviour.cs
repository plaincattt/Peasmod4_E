using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Peasmod4.Roles.Abilities;

[RegisterInIl2Cpp]
public class FootprintBehaviour : MonoBehaviour
{
    public SpriteRenderer renderer;
    public float maxLifeTime;
    public float remainingLifeTime;
    public Color color;
    
    public void Start()
    {
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        
        renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Utility.CreateSprite("Peasmod4.Resources.Other.Footprint.png", 256f);
        renderer.color = color;
    }

    public void Update()
    {
        remainingLifeTime -= Time.deltaTime;
        renderer.color = renderer.color.SetAlpha(1 / maxLifeTime * remainingLifeTime);
        if (remainingLifeTime <= 0f)
        {
            gameObject.Destroy();
        }
    }
}