using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace Game
{
    public struct ScaleSpriteData : IComponentData
    {
        public float Scale;
        public float TimeToUpdateScale;
    }

    public struct PositionSpriteData : IComponentData
    {
        public float TimeToUpdatePosition;
    }

    public struct ScaleTimerSpriteData : IComponentData
    {
        public float Value;
    }
    
    public struct PositionTimerSpriteData : IComponentData
    {
        public float Value;
    }

    public class SpriteAuthoring : MonoBehaviour
    {
        public float Size;
        public float TimeToUpdateScale;
        public float TimeToUpdatePosition;
        
        private class SpriteBaker : Baker<SpriteAuthoring>
        {
            public override void Bake(SpriteAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ScaleSpriteData
                {
                    Scale = authoring.Size, 
                    TimeToUpdateScale = authoring.TimeToUpdateScale,
                });
                AddComponent(entity, new PositionSpriteData
                {
                    TimeToUpdatePosition = authoring.TimeToUpdatePosition,
                });
                AddComponent<ScaleTimerSpriteData>(entity);
                AddComponent<PositionTimerSpriteData>(entity);
            }
        }
    }

    public partial struct SpriteTransformSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (transform, spriteData, timerSpriteData) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ScaleSpriteData>, RefRW<ScaleTimerSpriteData>>())
            {
                timerSpriteData.ValueRW.Value += deltaTime;

                if (timerSpriteData.ValueRO.Value >= spriteData.ValueRO.TimeToUpdateScale)
                {
                    transform.ValueRW.Scale = spriteData.ValueRO.Scale;
                    timerSpriteData.ValueRW.Value = 0;
                }
            }

            foreach (var (transform, spriteData, timerSpriteData) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PositionSpriteData>, RefRW<PositionTimerSpriteData>>())
            {
                timerSpriteData.ValueRW.Value += deltaTime;

                if (timerSpriteData.ValueRO.Value >= spriteData.ValueRO.TimeToUpdatePosition)
                {
                    transform.ValueRW.Position = new float3(Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f), 0f);
                    timerSpriteData.ValueRW.Value = 0;
                }
            }
        }
    }
}
