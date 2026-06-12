/// <summary>
/// Tehlikeli bölgelerdeki hasar animasyon tipleri.
/// Her zone'un kendi animasyon tipi olur.
/// </summary>
public enum HazardAnimationType
{
    /// <summary>
    /// Obje yukarıdan aşağıya düşer (avize, kaya, raf vb.)
    /// </summary>
    FallFromAbove,

    /// <summary>
    /// Obje öne/yana doğru devrilir (buzdolabı, gardırop vb.)
    /// </summary>
    Topple,

    /// <summary>
    /// Obje yerinde kırılır/parçalanır (cam pencere, ayna vb.)
    /// Scale ve alpha ile simüle edilir.
    /// </summary>
    Shatter,

    /// <summary>
    /// Ateş/alev sprite'ı büyüyerek belirir (ocak, gaz kaçağı vb.)
    /// </summary>
    Ignite
}
