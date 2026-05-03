using FFXIVClientStructs.FFXIV.Client.Game;

namespace Autominion;

public sealed class HousingLocation
{
    public HousingDistrict District { get; }
    public sbyte Ward { get; }
    public sbyte Plot { get; }
    public short Room { get; }
    public bool? IsInside { get; }

    public bool IsApartment => ApartmentWing != 0;
    public int ApartmentWing => Plot switch { -126 => 1, -127 => 2, _ => 0 };
    public int Division => ApartmentWing != 0 ? ApartmentWing : (Plot > 30 ? 2 : 1);
    public bool IsExteriorWardLocation => IsInside == false && Ward > 0;
    public bool IsOnPlot => IsInside == false && Plot > 0;
    public bool IsMeaningfulLocation =>
        (District != HousingDistrict.Unknown || Ward > 0) &&
        (IsApartment ? Room > 0 : IsInside == false ? Ward > 0 : Plot > 0);

    private HousingLocation(sbyte plot, sbyte ward, short room, HousingDistrict district, bool? isInside)
    {
        Plot = plot;
        Ward = ward;
        Room = room;
        District = district;
        IsInside = isInside;
    }

    public static HousingDistrict ConvertToDistrict(uint territory)
    {
        return territory switch
        {
            282 or 283 or 284 or 384 or 423 or 608 => HousingDistrict.Mist,
            342 or 343 or 344 or 385 or 425 or 609 => HousingDistrict.LavenderBeds,
            345 or 346 or 347 or 386 or 424 or 610 => HousingDistrict.Goblet,
            641 or 649 or 650 or 651 or 652 or 653 or 655 => HousingDistrict.Shirogane,
            979 or 980 or 981 or 982 or 983 or 984 or 999 => HousingDistrict.Empyreum,
            _ => HousingDistrict.Unknown,
        };
    }

    public static unsafe HousingLocation? FromCurrentLocation(uint territory)
    {
        try
        {
            var manager = HousingManager.Instance();
            if (manager == null)
            {
                return null;
            }

            return new HousingLocation(
                (sbyte)(manager->GetCurrentPlot() + 1),
                (sbyte)(manager->GetCurrentWard() + 1),
                manager->GetCurrentRoom(),
                ConvertToDistrict(territory),
                manager->IsInside());
        }
        catch
        {
            return null;
        }
    }

    public string GetLocationKey()
    {
        return $"{(int)District}:{Ward}:{Plot}:{Room}:{Division}:{IsInside}";
    }

    public override string ToString()
    {
        var inside = IsInside is null ? "unknown" : IsInside.Value ? "inside" : "outside";
        return IsApartment
            ? $"{District}, ward {Ward}, apartment room {Room}, division {Division}, {inside}"
            : $"{District}, ward {Ward}, plot {Plot}, division {Division}, {inside}";
    }
}
