using PrimalEngineEditor.Content;

namespace PrimalEngineEditor.AllEditors
{
    interface IAssetEditor
    {
        Asset Asset { get; }
        void SetAsset(Asset asset);
    }
}