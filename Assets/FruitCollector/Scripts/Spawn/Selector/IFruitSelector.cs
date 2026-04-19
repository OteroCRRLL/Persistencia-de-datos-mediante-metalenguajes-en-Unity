public interface IFruitSelector
{
    FruitData PickRandomFruitData();
    FruitData GetFruitDataById(string id);
}