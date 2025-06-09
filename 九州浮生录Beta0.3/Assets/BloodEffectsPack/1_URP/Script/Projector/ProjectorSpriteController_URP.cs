using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BloodEffectsPack
{
    public class ProjectorSpriteController_URP : MonoBehaviour
    {
  
        [Space(10)]
        public int columns = 1;
        public int rows = 1;
        public int frameLength = 1;
  

        private float tile_x = 1.0f;
        private float tile_y = 1.0f;
        private float offset_x = 0.0f;
        private float offset_y = 0.0f;

        private void Start()
        {
            tile_x = 1.0f / (float)rows;
            tile_y = 1.0f / (float)columns;
            SetProjector();
            SetFrameIndex(0);
        }
        private void Update()
        {
            
        }



        public void SetProjector()
        {
            UnityEngine.Rendering.Universal.DecalProjector projector
                   = GetComponentInChildren<UnityEngine.Rendering.Universal.DecalProjector>();
            projector.uvScale = new Vector2(tile_x, tile_y);
            projector.uvBias = new Vector2(offset_x, offset_y);
        }
        public void SetFrameIndex(int frame)
        {
            int frameIndex = frame % frameLength;
          
            int column = frameIndex % columns;
            int row = frameIndex / rows;

     
            Vector2 spriteSize;
            spriteSize.x = (float)1.0 / (float)columns;
            spriteSize.y = (float)1.0 / (float)rows;

            Vector2 spriteOffset;
            spriteOffset.x = column * spriteSize.x;
            spriteOffset.y = (1.0f - spriteSize.y) - ((float)row * spriteSize.y);


            UnityEngine.Rendering.Universal.DecalProjector projector
                      = GetComponentInChildren<UnityEngine.Rendering.Universal.DecalProjector>();
            projector.uvBias = new Vector2(spriteOffset.x, spriteOffset.y);

        }

        

    }
}


