using System.Collections.Generic;
using System;
using static TokenType;
public class Util
{
    public static List<byte> LEB128encode(int number)
    {
        var result = new List<byte>();

        while (true)
        {
            var b = (number & 0x7f);
            number >>= 7;

            if ((number == 0 && (b & 0x40) == 0) ||
                (number == -1 && (b & 0x40) != 0))
            {
                result.Add(Convert.ToByte(b));
                return result;
            }

            result.Add(Convert.ToByte(b | 0x80));
        }
    }

    // TODO move this elsewhere?
    public static byte OpToIntegerInstruction(Token operation)
    {
        switch (operation.Type)
        {
            case PLUS:
                return 0x6a;
            case MINUS:
                return 0x6b;
            case STAR:
                return 0x6c;
            case SLASH:
                return 0x6d;
            case PERCENT:
                return 0x6f;
            default:
                throw new Exception("not a operation" + operation);
        }
    }
}