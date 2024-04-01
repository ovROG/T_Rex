using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;


namespace SupSim_T_Rex
{
    internal static class TextureController
    {
        [HarmonyPatch(typeof(ProductLicenseManager), "LoadAllProducts")]
        [HarmonyPostfix]
        internal static void StartLicensePostfix(ProductLicenseManager __instance)
        {
            List<ProductLicenseSO> plSO = Traverse.Create(__instance).Field("m_Licenses").GetValue() as List<ProductLicenseSO>;

            List<Tuple<string, string>> brands = new();
            List<Tuple<int, string>> productNames = new();

            if (File.Exists($"{Paths.PluginPath}/T-Rex/Brands.txt"))
            {
                var lines = File.ReadAllLines($"{Paths.PluginPath}/T-Rex/Brands.txt");
                foreach (var line in lines)
                {
                    string[] names = line.Split('|');
                    brands.Add(new Tuple<string, string>(names[0], names[1]));
                }
            }

            foreach (ProductLicenseSO productLicenseSO in plSO)
            {
                foreach (ProductSO productSO in productLicenseSO.Products)
                {
                    Console.WriteLine("aa:" + productSO);
                    Renderer renderer = productSO.ProductPrefab.GetComponentInChildren<Renderer>(true);

                    if (renderer != null)
                    {
                        Console.WriteLine($"mat count: " + renderer.materials.Count());
                        foreach (Material mat in renderer.materials)
                        {
                            try
                            {
                                Console.WriteLine($"name Mat: " + mat.name);
                                if (mat.mainTexture != null)
                                {
                                    Texture tx = mat.mainTexture;
                                    string name = tx.name;
                                    Console.WriteLine($"name Tex: " + name);
                                    if (File.Exists($"{Paths.PluginPath}/T-Rex/Textures/{name}.png"))
                                    {
                                        Texture2D ntx = new(2, 2);
                                        byte[] texData = File.ReadAllBytes($"{Paths.PluginPath}/T-Rex/Textures/{name}.png");
                                        ntx.LoadImage(texData);
                                        mat.mainTexture = ntx;
                                        Console.WriteLine("file:" + name);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Unable Patch material:" + mat);
                                Console.WriteLine(ex);
                            }
                        }
                    }

                    try
                    {
                        Sprite icon = productSO.ProductIcon;
                        string name = icon.texture.name;
                        Console.WriteLine("name:" + name);
                        if (File.Exists($"{Paths.PluginPath}/T-Rex/Icons/{name}.png"))
                        {
                            Texture2D ntx = new(icon.texture.width, icon.texture.height, icon.texture.format, icon.texture.mipmapCount, false);
                            byte[] texData = File.ReadAllBytes($"{Paths.PluginPath}/T-Rex/Icons/{name}.png");
                            ntx.LoadImage(texData);
                            Graphics.CopyTexture(ntx, icon.texture);
                            Console.WriteLine("file:" + name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable Patch sprite:" + productSO);
                        Console.WriteLine(ex);
                    }

                    try
                    {
                        productSO.ProductBrand = brands.First(item => item.Item1 == productSO.ProductBrand).Item2;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable Patch Brand:" + productSO);
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LocalizationManager), "InitLocalization")]
        [HarmonyPostfix]
        internal static void StartLocalizationPostfix(LocalizationManager __instance)
        {
            Dictionary<int, string> LPN = Traverse.Create(__instance).Field("m_LocalizedProductNames").GetValue() as Dictionary<int, string>;

            if (File.Exists($"{Paths.PluginPath}/T-Rex/Products.txt"))
            {
                var lines = File.ReadAllLines($"{Paths.PluginPath}/T-Rex/Products.txt");
                foreach (var line in lines)
                {
                    string[] names = line.Split('|');
                    LPN[Int32.Parse(names[0])] = names[1];
                }
            }
        }
    }


    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Harmony hPatchControllerStart;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Start()
        {
            try
            {
                hPatchControllerStart = Harmony.CreateAndPatchAll(typeof(TextureController), "TextureController");
            }
            catch (Exception e)
            {
                Logger.LogInfo(e.Message);
            }
        }
    }
}
