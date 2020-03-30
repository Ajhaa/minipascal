public class WASMbase
{
    public static byte[] Header = new byte[] {
        0x00, 0x61, 0x73, 0x6d, // wasm magic
        0x01, 0x00, 0x00, 0x00, // wasm version number
    };

    // need to import three things from js
    // the linear memory, and two i/o functions

    // TODO remember to change import number
    public static byte[] Import = new byte[] {
        0x02, 0x0b, 0x01, // section code, size and number of imports

        0x02, 0x6a, 0x73, // first import from 'js'
        0x03, 0x6d, 0x65, 0x6d, // 'mem'
        0x02, 0x00, 0x04, // initial mem size of at least 4 pages (256KiB)

        // 0x02, 0x6a, 0x73, // second import from 'js'
        // 0x04, 0x72, 0x65, 0x61, 0x64, // 'read'
        // 0x00, 0x00,

        // 0x02, 0x6a, 0x73, // third import from 'js'
        // 0x05, 0x77, 0x72, 0x69, 0x74, 0x65, // 'write'
        // 0x00, 0x01
    };
}