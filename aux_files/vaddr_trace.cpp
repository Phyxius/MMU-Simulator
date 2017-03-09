#include <stdio.h>
#include "pin.H"

int pid;

FILE * trace;

// Print a memory read record
VOID MemRead(VOID * ip, VOID * addr)
{
    fprintf(trace,"%6u R %p\n", pid, addr);
}

// Print a memory write record
VOID MemWrite(VOID * ip, VOID * addr)
{
    fprintf(trace,"%6u W %p\n", pid, addr);
}

VOID InstrFetch(VOID *ip)
{
    fprintf(trace, "%6u I %p\n", pid, ip);
}

// Is called for every instruction and instruments reads and writes
VOID Instruction(INS ins, VOID *v)
{
    // Insert a call to printip before every instruction, and pass it the IP
    INS_InsertCall(ins,
                   IPOINT_BEFORE,
                   (AFUNPTR)InstrFetch,
                   IARG_INST_PTR,
                   IARG_END);

    // instruments loads using a predicated call, i.e.
    // the call happens iff the load will be actually executed
    // (this does not matter for ia32 but arm and ipf have predicated instructions)
    if (INS_IsMemoryRead(ins))
    {
        INS_InsertPredicatedCall(
            ins, IPOINT_BEFORE, (AFUNPTR)MemRead,
            IARG_INST_PTR,
            IARG_MEMORYREAD_EA,
            IARG_END);
    }

    // instruments stores using a predicated call, i.e.
    // the call happens iff the store will be actually executed
    if (INS_IsMemoryWrite(ins))
    {
        INS_InsertPredicatedCall(
            ins, IPOINT_BEFORE, (AFUNPTR)MemWrite,
            IARG_INST_PTR,
            IARG_MEMORYWRITE_EA,
            IARG_END);
    }
}

VOID Fini(INT32 code, VOID *v)
{
    fprintf(trace, "#eof\n");
    fclose(trace);
}


int main(int argc, char *argv[])
{
    pid = getpid();
    PIN_Init(argc, argv);

    trace = fopen("pinatrace.out", "w");

    // Register Instruction to be called to instrument instructions
    INS_AddInstrumentFunction(Instruction, 0);

    // Register Fini to be called when the application exits
    PIN_AddFiniFunction(Fini, 0);

    // Never returns
    PIN_StartProgram();
    
    return 0;
}
