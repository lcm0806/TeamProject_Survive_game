using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class CraftingManager : Singleton<CraftingManager>
{
    [SerializeField] List<Recipe> allRecipes;

    void Awake()
    {
        SingletonInit();
    }

    //public bool TryCraft(Recipe recipe)
    //{
    //    if (!HasEnoughMaterials(recipe))
    //    {

    //    }
    //}

    //public bool HasEnoughMaterials(Recipe recipe)
    //{
    //    foreach(var material in recipe.requireMaterials)
    //    {

    //    }
    //}
}
