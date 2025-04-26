using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    public class InitialSpawnerComponentAuthoring : MonoBehaviour {
        public List<int2> CellCoordinates;
        
        private class InitialSpawnerComponentAuthoringBaker : Baker<InitialSpawnerComponentAuthoring> {
            public override void Bake(InitialSpawnerComponentAuthoring authoring) {
                var builder = new BlobBuilder(Allocator.Temp);
                ref CellCoordinates coordinatesRoot = ref builder.ConstructRoot<CellCoordinates>();
                BlobBuilderArray<int2> cellCoordinates = builder.Allocate(ref coordinatesRoot.Coordinates, authoring.CellCoordinates.Count);
                for (int i = 0; i < authoring.CellCoordinates.Count; i++) {
                    cellCoordinates[i] = authoring.CellCoordinates[i];
                }

                var coordinatesRef = builder.CreateBlobAssetReference<CellCoordinates>(Allocator.Persistent);
                builder.Dispose();
                AddBlobAsset<CellCoordinates>(ref coordinatesRef, out var hash);
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new InitialSpawnerComponent() {
                    Coordinates = coordinatesRef
                });
            }
        }
    }
    
    public struct InitialSpawnerComponent : IComponentData {
        public BlobAssetReference<CellCoordinates> Coordinates;
    }

    public struct CellCoordinates {
        public BlobArray<int2> Coordinates;
    }
}