namespace WheelWizard.WiiManagement.Domain.Mii.Custom;

//this value is only stored in 3 bits, so there can be only 8 values
public enum MiiPreferredFacialExpression : uint
{
    None = 0,
    FacialExpression1 = 1,
    FacialExpression2 = 2,
    FacialExpression3 = 3,
    FacialExpression4 = 4,
    FacialExpression5 = 5,
    FacialExpression6 = 6,
    FacialExpression7 = 7,
}
