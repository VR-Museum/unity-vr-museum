using System;

namespace src.museum.quiz.model.item
{
    public static class MineralNameConverter
    {
        public static string Convert(MineralName mineralName)
        {
            switch (mineralName)
            {
                case MineralName.Aragonite : return "Арагонит \n(CaCO3)";
                case MineralName.Calcite : return "Кальцит \n(CaCO3)";
                case MineralName.Halite : return "Галит \n(NaCl)";
                case MineralName.Marcacite : return "Марказит \n(FeS2)";
                case MineralName.Pseudomalachite : return "Псевдомалахит \n(Cu5(PO4)2(OH)4)";
                case MineralName.GrenatAlmandin : return "Гранат Альмандин \n(Fe3Al2[SiO4]3)";
                case MineralName.Vanadinite : return "Ванадинит \n(Pb5[VO4]3Cl)";
                case MineralName.Aurichalcite : return "Аурихальцит \n((Zn,Cu)5(CO3)2(OH)6)";
                case MineralName.Antimonite : return "Антимонит \n(Sb2S3)";
                case MineralName.GypseRose : return "Гипсовая роза \n(CaSO4·2H2O)";
                case MineralName.Slate : return "Сланец";
                case MineralName.Amethyst : return "Аметист \n(SiO2)";
                case MineralName.Fluorite : return "Флюорит \n(CaF2)";
                case MineralName.Sfaleryte : return "Сфалерит \n(ZnS)";
                case MineralName.Tourmaline : return "Турмалин \n(Na(Li,Al)3Al6[(OH)4|(BO3)3Si6O18])";
                case MineralName.Kianite : return "Кианит \n(Al2O(SiO4))";
                case MineralName.Azurite : return "Азурит \n(Cu3(CO3)2(OH)2)\n(CuCO3)2·Cu(OH)2";
                case MineralName.Agate : return "Агат \n(SiO2)";
                case MineralName.Apatite : return "Апатит \n(Са5[PO4]3(F, Cl, ОН))";
                case MineralName.Epidote : return "Эпидот \n(Ca2Al2Fe-III(SiO4)3OH)";
                case MineralName.Goethite : return "Гётит \n(FeO(OH))";
                case MineralName.Jasper : return "Яшма \n(SiO2)";
                case MineralName.Obsidian : return "Обсидиан";
                case MineralName.Chrysoprase : return "Хризопраз \n(SiO2)";
                case MineralName.Citrine : return "Цитрин \n(SiO2)";
                case MineralName.Diopside : return "Диопсид \n(CaMg(Si2O6))";
                case MineralName.Magnetite : return "Магнетит \n(Fe3O4)\n(FeO·Fe2O3)";
                case MineralName.Talc : return "Тальк \n(Mg3Si4O10(OH)2)";
                case MineralName.Actinolite : return "Актинолит \n(Ca2(Mg,Fe)5[Si8O22](OH)2)";
                case MineralName.Sapphire : return "Сапфир \n(Al2O3)";
                case MineralName.Topaz : return "Топаз \n(Al2[SiO4](F,OH)2)";
            }

            throw new Exception("Unexpected mineral name");
        }
    }
}