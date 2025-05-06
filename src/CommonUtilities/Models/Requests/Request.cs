namespace CommonUtilities.Models.Requests;

/// <summary>
/// General, abstract model of a request.
/// </summary>
/// <remarks>
/// Requests derivative to this type are used during communication between client and server.
/// </remarks>
public abstract record Request
{
    #region Properties
    public string Type
    {
        get
        {
            return GetType().Name;
        }
    }
    #endregion
}
