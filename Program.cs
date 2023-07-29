using System.IO;

namespace H5_swizzle_pattern_mapper
{
    internal class Program
    {



        static void Main(string[] args){



            Console.WriteLine("starting process");
            // easy, the 2, 6, 22, 262 pattern
            //string pattern_file = @"C:\Users\Joe bingle\Downloads\Finalized_Swizzle_Patterns_7-19-2023\[chunk4][16 Mb][32 byte][4096x4096][FinalPattern].txt";
            // trickier, doesn't follow the square/power pattern, (xor pattern)
            //string pattern_file = @"C:\Users\Joe bingle\Downloads\Finalized_Swizzle_Patterns_7-19-2023\[chunk4][16 Mb][32byte][4096x4096[FinalPattern2].txt";
            //string OUT_bitmappings_file = pattern_file + ".bitmappings.txt";

            string directory = @"C:\Users\Joe bingle\Downloads\Finalized_Swizzle_Patterns_7-19-2023\";
            DirectoryInfo d = new DirectoryInfo(directory);

            foreach (var file in Directory.GetFiles(directory))
                generate_bitmapping(file);
            



            Console.WriteLine("process complete, press enter to exit");
            Console.ReadLine();
        }
        static void generate_bitmapping(string swizpattern_file){

            Console.WriteLine("swizzle patterning \"" + swizpattern_file + "\"");
            // configure mappings array
            long[] bit_mappings = new long[32]; // assume max pixel count is int32.max
            for (int i = 0; i < bit_mappings.Length; i++)
                bit_mappings[i] = 0; // 1L << i; // since we dont know these ones, it would be pointless to map them

            long current_pixel = 0;
            long current_xbox_pixel = 0;

            using (TextReader reader = File.OpenText(swizpattern_file))
            {
                while (true)
                {
                    string? line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;

                    // read line to get offset of xbox pixel to current PC pixel
                    current_xbox_pixel += long.Parse(line);






                    long bit_index = power_of_2_bit_index(current_pixel);
                    if (bit_index != -1)
                    {
                        //long rearranged_bit_index = power_of_2_bit_index(current_xbox_pixel);
                        //if (rearranged_bit_index == -1)
                        //    throw new Exception("xbox pixel is not square index, when supposed to be");

                        // map this pixel
                        bit_mappings[bit_index] = current_xbox_pixel; // xbox patterns will not always be square numbers, they can actually overlap
                    }
                    else
                    { // we can actually validate the value of the xbox pixel for this line
                        long actual_xbox_pixel = swizzle(current_pixel, bit_mappings);
                        if (actual_xbox_pixel != current_xbox_pixel)
                        {
                            Console.WriteLine("line " + current_pixel + " was wrong");
                            current_xbox_pixel = actual_xbox_pixel;
                        }
                    }

                    current_pixel++;
                }
            }
            // then write out the patterns
            string output_dir = Path.GetDirectoryName(swizpattern_file) + "\\bitmappings\\";
            System.IO.Directory.CreateDirectory(output_dir);
            using (StreamWriter outputFile = new StreamWriter(output_dir + Path.GetFileName(swizpattern_file) + ".txt")){
                outputFile.WriteLine("uint64_t* bit_array = new uint64_t[32]{");
                for (long i = 0; i < bit_mappings.Length; i++)
                    outputFile.WriteLine("0b" + Convert.ToString(bit_mappings[i], 2) + ",");
                outputFile.WriteLine("};");
            }
        }
        static long power_of_2_bit_index(long test){
            long bit_index = -1; // from the right side
            while (test != 0){
                if ((test & 1) == 1) // if the last bit is checked
                    if (test > 1)
                        return -1; // contains a non-square bit
                    else return bit_index+1; // this is the last bit, meaning yes it is square

                test >>= 1;
                bit_index++;
            }
            return bit_index; // this should only occur with 0x0
        }
        //static long unswizzle(long pixel_index, long[] bit_mappings)
        //{
        //
        //}
        static long swizzle(long pixel_index, long[] bit_mappings){
            long output = 0;
            for (long i =0; i < bit_mappings.Length; i++) // we should use a different value to measure how far we've gotten so far
                if ((pixel_index & (1 << (int)i)) != 0)
                    output ^= bit_mappings[i];
            return output;
        }
    }
}