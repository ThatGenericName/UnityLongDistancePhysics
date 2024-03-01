using System.Runtime.CompilerServices;

namespace ObjectZoning
{
    public interface IStructAllocatorObject
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init();

        public void Destroy();
        public int StructID { get; set; }
    }
}