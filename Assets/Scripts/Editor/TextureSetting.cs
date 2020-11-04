using UnityEditor;

public class TextureSetting : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
    }
}