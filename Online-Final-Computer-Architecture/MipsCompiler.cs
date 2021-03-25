using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Online_Final_Computer_Architecture
{
    class MipsCompiler
    {
        private readonly Configuration _configuration;

        private List<string> _commands = new List<string>();
        private List<HazardConfirmation> _potentialHazards = new List<HazardConfirmation>();


        public MipsCompiler(Configuration configuration)
        {
            _configuration = configuration;
        }


        public Solution ExecuteCommands()
        {
            var tokens = SplitCommandsIntoTokens(_commands);

            var pipelineNoForwardingUnit = CreateBasePipeline(_commands.Count);
            var pipelineWithForwardingUnit = CreateBasePipeline(_commands.Count);

            TestForHazards(pipelineNoForwardingUnit, tokens, false);
            TestForHazards(pipelineWithForwardingUnit, tokens, true);

            var solution = new Solution();

            solution.Pipelines.Add(pipelineNoForwardingUnit);
            solution.Pipelines.Add(pipelineWithForwardingUnit);
            

            return solution;
        }

        private List<List<string>> SplitCommandsIntoTokens(List<string> commands)
        {
            List<List<string>> parsedCommands = new List<List<string>>();
            var separators = new[] { ' ', ',', '(', ')' };

            foreach (var cmd in commands)
            {
                parsedCommands.Add(cmd.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
            return parsedCommands;
        }

        private void TestForHazards(List<List<string>> pipeline, List<List<string>> commands, bool isForwarding)
        {
            StructuralHazardTest(pipeline, commands);
            DataHazardTest(pipeline, commands, isForwarding);
        }

        private void DataHazardTest(List<List<string>> pipeline, List<List<string>> commands, bool isForwarding)
        {
            //First instruction is always a gimme
            

            //Compare each level of the instructions
            for (int i = 1; i < commands.Count; i++)
            {
                var currentInstruction = commands[i];
                var currentInstructionIndex = commands.IndexOf(currentInstruction);

                for (int j = 0; j < currentInstructionIndex; j++)
                {
                    var instructionToCompare = commands[j];
                    var instructionToCompareIndex = commands.IndexOf(instructionToCompare);

                    
                    var rawHazardConfirmation = ReadAfterWriteHazardCheck(instructionToCompare, currentInstruction, 
                                                                        instructionToCompareIndex,currentInstructionIndex, pipeline, isForwarding);
                        
                    var warHazardConfirmation = WriteAfterReadHazardCheck(instructionToCompare, currentInstruction,
                                                                        instructionToCompareIndex, currentInstructionIndex, pipeline);

                    var wawHazardConfirmation = WriteAfterWriteHazardCheck(instructionToCompare, currentInstruction,
                                                                        instructionToCompareIndex, currentInstructionIndex, pipeline);

                    //Add all hazard confirmations for the current instruction
                    //if (_potentialHazards.ContainsKey(i))
                    //{
                    //    _potentialHazards[i].Add(rawHazardConfirmation);
                    //    _potentialHazards[i].Add(warHazardConfirmation);
                    //    _potentialHazards[i].Add(wawHazardConfirmation);
                    //}
                    //else
                    //{
                    //    _potentialHazards.Add(i, new List<HazardConfirmation> { rawHazardConfirmation, warHazardConfirmation, warHazardConfirmation });
                    //}
                    
                    //Redraw the pipeline
                    RecreatePipeline(currentInstructionIndex, currentInstruction[0], instructionToCompare[0], pipeline, rawHazardConfirmation, isForwarding);

                    //if (rawHazardConfirmation.IsHazard) RecreatePipeline(currentInstructionIndex, pipeline, rawHazardConfirmation);
                    

                    //var warHazardConfirmation = WriteAfterReadHazardCheck(instructionToCompare, currentInstruction, 
                    //                                                      instructionToCompareIndex, currentInstructionIndex, pipeline);

                    //if(warHazardConfirmation.IsHazard) RecreatePipeline(currentInstructionIndex, pipeline, warHazardConfirmation);

                    //var wawHazardConfirmation = WriteAfterWriteHazardCheck(instructionToCompare, currentInstruction,
                    //                                                       instructionToCompareIndex, currentInstructionIndex, pipeline);

                    //if (wawHazardConfirmation.IsHazard) RecreatePipeline(currentInstructionIndex, pipeline, wawHazardConfirmation);


                    //Output all hazards: RAW, WAR, WAW
                    //Console.Write($"{currentInstruction[0]}: {rawHazardConfirmation.Name}  ");
                    //Console.WriteLine($"Registers: {rawHazardConfirmation.Message}");

                    //Console.Write($"{currentInstruction[0]}: {warHazardConfirmation.Name}  ");
                    //Console.WriteLine($"Registers: {warHazardConfirmation.Message}");

                    //Console.Write($"{currentInstruction[0]}: {wawHazardConfirmation.Name}  ");
                    //Console.WriteLine($"Registers: {wawHazardConfirmation.Message}");
                }
            }
        }

        private HazardConfirmation ReadAfterWriteHazardCheck(List<string> instructionToCompare, List<string> currentInstruction, int comparePipeIndex, int currentPipeIndex, List<List<string>> pipeline, bool isForwarding)
        {
            var hazardConfirmation = new HazardConfirmation();
            string destReg;

            //Determine what the source register is for the "instructionToCompare"
            if (instructionToCompare[0].Equals("sw", StringComparison.OrdinalIgnoreCase))
                destReg = instructionToCompare[3];
            else 
                destReg = instructionToCompare[1];

            //Now take our current instruction and compare the registers that are being read against the destReg
            var regsToCompare = new List<string>();

            //if ((currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || currentInstruction[0].Equals("lw", StringComparison.OrdinalIgnoreCase)) &&
            //    (instructionToCompare[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || instructionToCompare[0].Equals("lw", StringComparison.OrdinalIgnoreCase)))
            //{
            //    regsToCompare.Add(currentInstruction[3]);
            //}
            if ((currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || currentInstruction[0].Equals("lw", StringComparison.OrdinalIgnoreCase)) 
                    && destReg.Equals(currentInstruction[1]))
            {
                regsToCompare.Add(currentInstruction[3]);
            }
            else 
            {
                regsToCompare.Add(currentInstruction[2]);
                regsToCompare.Add(currentInstruction[3]);
            }

            foreach (var reg in regsToCompare)
            {
                if (reg.Equals(destReg, StringComparison.OrdinalIgnoreCase))
                {
                    hazardConfirmation.Registers.Add(reg);
                }
            }

            //Add final results to the hazard confirmation
            hazardConfirmation.IsHazard = hazardConfirmation.Registers.Count > 0 ? true : false;
            hazardConfirmation.Name = hazardConfirmation.IsHazard ? "RAW" : "None";

            //Determine amount of stalls based on where reg is needed versus available
            if (isForwarding)
            {
                //Determine where we are forwarding from based on the instruction
                //If its regular arithmetic instruction, we can forward from ALU
                //If its Load/store, we can forward from the memory stage
                if (instructionToCompare[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || instructionToCompare[0].Equals("lw", StringComparison.OrdinalIgnoreCase))
                {
                    var fwdIndex = pipeline[comparePipeIndex].IndexOf("M");

                    //Now find out where we can forward it to
                    if (currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || currentInstruction[0].Equals("lw", StringComparison.OrdinalIgnoreCase))
                    {
                        //Need to determine if destination register and decoded registers match for lw/sw to lw/sw instructions
                        var regToCompare = currentInstruction[3];

                        //We know that a sw or lw that just happened needs to recalculate its address in a proceeding instruction due to offset
                        if (destReg.Equals(regToCompare))
                        {
                            //data will be needed as early as the "execution" stage
                            var executeIndex = pipeline[currentPipeIndex].IndexOf("E");
                            if (fwdIndex == executeIndex)
                                hazardConfirmation.StallCount = 1;
                        }
                    }
                    else
                    {
                        int currentPipeStageIndex = pipeline[currentPipeIndex].IndexOf("E");
                        if (currentPipeStageIndex == fwdIndex)
                            hazardConfirmation.StallCount = 1;
                        else if(currentPipeStageIndex < fwdIndex)
                            hazardConfirmation.StallCount = Math.Abs(currentPipeStageIndex - fwdIndex);
                    }
                }
                else 
                {
                    var fwdIndex = pipeline[comparePipeIndex].IndexOf("E");
                    //Now find out where we can forward it to
                    if (currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase) || currentInstruction[0].Equals("lw", StringComparison.OrdinalIgnoreCase))
                    {
                        var regToCompare = currentInstruction[3];

                        //We know that a sw or lw may need to calculate the address based on a register written to in add/sub
                        //This will generally not get hit
                        if (destReg.Equals(regToCompare))
                        {
                            //data will be needed as early as the "execution" stage
                            var executeIndex = pipeline[currentPipeIndex].IndexOf("E");
                            if (fwdIndex == executeIndex)
                                hazardConfirmation.StallCount = 1;
                        }
                    }
                    else
                    {
                        int currentPipeStageIndex = pipeline[currentPipeIndex].IndexOf("E");
                        if (currentPipeStageIndex == fwdIndex)
                            hazardConfirmation.StallCount = 1;
                        else if (currentPipeStageIndex < fwdIndex) //This case should never happen... forwarding can't go backwards like this
                            hazardConfirmation.StallCount = Math.Abs(currentPipeStageIndex - fwdIndex);
                    }
                } 
                
            }
            else 
            {
                var pipeStageToCompareIndex = pipeline[comparePipeIndex].IndexOf("W");
                var currentPipeStageIndex = pipeline[currentPipeIndex].IndexOf("D");
                if (pipeStageToCompareIndex > currentPipeStageIndex) //Only get stalls if the arrow is to the left, anything equal or to the right doesn't need a stall
                    hazardConfirmation.StallCount = Math.Abs(currentPipeStageIndex - pipeStageToCompareIndex);
            }
  
            //Show what registers are affected
            foreach (var reg in hazardConfirmation.Registers) 
            {
                hazardConfirmation.Message += $"{reg} ";
            }

            return hazardConfirmation;
        }

        private HazardConfirmation WriteAfterReadHazardCheck(List<string> instructionToCompare, List<string> currentInstruction, int comparePipeIndex, int currentPipeIndex, List<List<string>> pipeline) 
        {
            var hazardConfirmation = new HazardConfirmation();
            string destReg;

            //Determine what the source register is for the "currentInstruction"
            if (currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase))
            {
                destReg = currentInstruction[3];
            }
            else
            {
                destReg = currentInstruction[1];
            }

            //Now take our "instructionToCompare" and compare the registers against the destReg of currentInstruction
            var regsToCompare = new List<string>();

            if (instructionToCompare[0].Equals("sw", StringComparison.OrdinalIgnoreCase))
            {
                regsToCompare.Add(instructionToCompare[1]);
            }
            else
            {
                regsToCompare.Add(instructionToCompare[2]);
                regsToCompare.Add(instructionToCompare[3]);
            }

            foreach (var reg in regsToCompare)
            {
                if (reg.Equals(destReg, StringComparison.OrdinalIgnoreCase))
                {
                    hazardConfirmation.Registers.Add(reg);
                }
            }

            //Add final results to the hazard confirmation
            hazardConfirmation.IsHazard = hazardConfirmation.Registers.Count > 0 ? true : false;
            hazardConfirmation.Name = hazardConfirmation.IsHazard ? "WAR" : "None";

            //Determine amount of stalls based on where reg is needed versus available
            var pipeToCompare = pipeline[comparePipeIndex].IndexOf("D");
            var currentPipe = pipeline[currentPipeIndex].IndexOf("W");
            if (pipeToCompare > currentPipe) //Only get stalls if the arrow is to the left, anything equal or to the right doesn't need a stall
                hazardConfirmation.StallCount = Math.Abs(currentPipe - pipeToCompare);

            //Show what registers are affected
            foreach (var reg in hazardConfirmation.Registers)
            {
                hazardConfirmation.Message += $"{reg} ";
            }
            return hazardConfirmation;
        }

        private HazardConfirmation WriteAfterWriteHazardCheck(List<string> instructionToCompare, List<string> currentInstruction, int comparePipeIndex, int currentPipeIndex, List<List<string>> pipeline) 
        {
            var hazardConfirmation = new HazardConfirmation();
            string destRegForCurrent;
            string destRegForCompare;

            //Determine what the destination register is for the "currentInstruction"
            if (currentInstruction[0].Equals("sw", StringComparison.OrdinalIgnoreCase))
            {
                destRegForCurrent = currentInstruction[3];
            }
            else
            {
                destRegForCurrent = currentInstruction[1];
            }

            //Determine what the destination register is for the "compareInstruction"
            if (instructionToCompare[0].Equals("sw", StringComparison.OrdinalIgnoreCase))
            {
                destRegForCompare = instructionToCompare[3];
            }
            else
            {
                destRegForCompare = instructionToCompare[1];
            }

            if (destRegForCurrent.Equals(destRegForCompare, StringComparison.OrdinalIgnoreCase)) 
            {
                hazardConfirmation.Registers.Add(destRegForCurrent);
            }

            //Add final results to the hazard confirmation
            hazardConfirmation.IsHazard = hazardConfirmation.Registers.Count > 0 ? true : false;
            hazardConfirmation.Name = hazardConfirmation.IsHazard ? "WAW" : "None";

            //No stalls needed in this case, depends on system... goes based on general pipeline
            hazardConfirmation.StallCount = 0;

            //Show what registers are affected
            foreach (var reg in hazardConfirmation.Registers)
            {
                hazardConfirmation.Message += $"{reg} ";
            }
            return hazardConfirmation;
        }

        private void StructuralHazardTest(List<List<string>> pipeline, List<List<string>> commands)
        {
            //Structural Hazard Test
            //Check to see if resources are being used at the same time in the pipeline
            //Check for each pipe in the pipeline
            //First pipe can always be skipped
            for (int i = 1; i < pipeline.Count; i++)
            {
                var currentPipe = pipeline[i];
                var currentPipeIndex = pipeline.IndexOf(currentPipe);

                for (int j = 0; j < currentPipeIndex; j++)
                {
                    var pipeToCompare = pipeline[j];
                    var pipeToCompareIndex = pipeline.IndexOf(pipeToCompare);

                    for (int k = 0; k < currentPipe.Count; k++)
                    {
                        int pipeToCompareContentIndex = currentPipeIndex + k;

                        if (pipeToCompareContentIndex < pipeToCompare.Count)
                        {
                            //This checks to see if pipeline stages overlap
                            //Ex. Fetch stage in pipe 1 overlaps Fetch stage in pipe 0
                            if (currentPipe[k + currentPipeIndex].Equals(pipeToCompare[pipeToCompareContentIndex]))
                            {
                                Console.WriteLine($"Pipe stage matches at Current Pipe {currentPipe[k + currentPipeIndex]} and PipeToCompare {pipeToCompare[pipeToCompareContentIndex]}");

                                //Will need to store the dependency message somewhere for the current pipe facing the dependency
                            }

                            //Now we need to check if instruction types require the same resource
                            if (currentPipe[k + currentPipeIndex].Equals("F") && pipeToCompare[pipeToCompareContentIndex].Equals("M") &&
                                (commands[pipeToCompareIndex][0].Equals("lw", StringComparison.OrdinalIgnoreCase) || commands[pipeToCompareIndex][0].Equals("sw", StringComparison.OrdinalIgnoreCase)))
                            {
                                Console.WriteLine($"Structural Hazard at Pipe {currentPipeIndex} due to Pipe {pipeToCompareIndex} instruction");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Done checking Current Pipe: {currentPipeIndex} against Pipe: {pipeToCompareIndex}");
                            break;
                        }
                    }
                }
            }
        }

        public List<List<string>> CreateBasePipeline(int instructionCount)
        {
            var pipeline = new List<List<string>>();

            for (int i = 0; i < instructionCount; i++)
            {
                var newPipe = new List<string>();

                for (int j = 0; j < i; j++)
                {
                    newPipe.Add(" ");
                }

                newPipe.Add("F");
                newPipe.Add("D");
                newPipe.Add("E");
                newPipe.Add("M");
                newPipe.Add("W");

                pipeline.Add(newPipe);
            }
            return pipeline;
        }

        public void RecreatePipeline(int instructionToUpdateIndex, string currentInstruction, string instructionToCompare, List<List<string>> pipeline, HazardConfirmation hazardConfirmation, bool isForwarding) 
        {
            var pipeToUpdate = pipeline[instructionToUpdateIndex];

            if (hazardConfirmation.Name.Equals("RAW"))
            {
                if (isForwarding)
                {
                    //We know stalls go before "execute" in forwarding since we go back to ALU
                    var decodeIndex = pipeToUpdate.IndexOf("D");
                    var stallStartIndex = decodeIndex + 1;

                    for (int i = 0; i < hazardConfirmation.StallCount; i++) 
                    {
                        //Include the stalls into the pipe
                        pipeToUpdate.Insert(stallStartIndex, "S");
                    }
                }
                else 
                {
                    //Find the fetch index and insert at 1 index after to delay decoding
                    var fetchIndex = pipeToUpdate.IndexOf("F");
                    var stallStartIndex = fetchIndex + 1;

                    for (int i = 0; i < hazardConfirmation.StallCount; i++)
                    {
                        //Include the stalls into the pipe
                        pipeToUpdate.Insert(stallStartIndex, "S");
                    }
                }
                //Now fix all instructions below the updated pipe
                int shiftInstructionCount = pipeToUpdate.IndexOf("D");
                for (int j = instructionToUpdateIndex + 1; j < pipeline.Count; j++)
                {
                    var currentPipe = pipeline[j];
                    if (currentPipe.Count > 0) currentPipe.Clear();
                    for (int k = 0; k < shiftInstructionCount; k++)
                    {
                        currentPipe.Add(" ");
                    }
                    currentPipe.Add("F");
                    currentPipe.Add("D");
                    currentPipe.Add("E");
                    currentPipe.Add("M");
                    currentPipe.Add("W");
                    shiftInstructionCount++;
                }
            }
            
            if (hazardConfirmation.Name.Equals("WAR"))
            {
                //Nothing will happen in a regular pipeline
                //Writeback will always be 4 cycles away from decode
                //Only some cases where this will happen, for purpose of project
                //we assume that there are no early writes, late reads, or out-of-order execution
            }
            
            if (hazardConfirmation.Name.Equals("WAW")) 
            {
                //Nothing will happen in a regular pipeline
                //We are assuming instructions that the reordering won't mess up the value in registers
                //Only some cases where this will happen, for purpose of project
                //we assume that there register writes are in order
            }

            if (hazardConfirmation.Name.Equals("Structural Hazard")) 
            {
                
            }
        }

        //public CommandValidation IsValidCommand(string command)
        //{
        //    var tokens = SplitCommandsIntoTokens(command);

        //    //Check the instruction first before going over parameters
        //    var instruction = tokens[0].ToInstruction(_configuration);

        //    //If its not a valid instruction, its not a valid command
        //    if (instruction == null) return new CommandValidation() { IsValid = false, Message = "Invalid Instruction. Try again:" };

        //    //Check to see which format the command falls under, and check for validity
        //    if (instruction.Format.Equals("C")) return IsValidCFormat(tokens);
        //    else if (instruction.Format.Equals("I")) return IsValidIFormat(tokens);
        //    else return IsValidMFormat(tokens);
        //}

        public void QueueCommand(string command)
        {
            _commands.Add(command);
        }
    }
}
