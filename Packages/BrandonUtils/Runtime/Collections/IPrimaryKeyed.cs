namespace Packages.BrandonUtils.Runtime.Collections {
    public interface IPrimaryKeyed<out T> {
        T PrimaryKey { get; }
    }
}