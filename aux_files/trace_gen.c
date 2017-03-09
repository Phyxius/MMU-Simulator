#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>

unsigned int num_references;
unsigned int mem_stride;
unsigned int num_procs;
unsigned int max_proc_size;
unsigned int slice;
float rw_ratio;
char * ref_pattern;

void proc_args( int iargc, char ** iargv );

void generate_write_ref();
void generate_read_ref();

typedef struct{
    int id;
    unsigned int last_ref;
} proc_t;

int main( int argc, char ** argv )
{
    unsigned int i, nreads=0, nwrites=0, cur_idx, addr; 
    float f;
    char op;
    proc_t * procs, * p;

    srand48( 5 );
    srand( 5 );
    proc_args( argc, argv );

    fprintf(stderr, "Number of references: %u\n", num_references );
    fprintf(stderr, "Number of processes: %u\n", num_procs );
    fprintf(stderr, "Maximum process size: %u\n", max_proc_size );

    fprintf(stderr, "Memory reference pattern: %s ", ref_pattern );
    if( strcmp(ref_pattern, "sequential") == 0 ) {
        fprintf(stderr, "(%u byte stride)", mem_stride );
    }
    fprintf(stderr, "\nR/W ratio: %.2f\n", rw_ratio );

    procs = (proc_t *)malloc( num_procs * sizeof(proc_t ) );
    for( i=0; i<num_procs; i++ ) {
        procs[i].id = i;
        procs[i].last_ref = 0;
    }

    cur_idx = 0;
    for( i=0; i < num_references; i++ ) {
        if( i % slice == 0 ) {
            cur_idx = (cur_idx+1) % num_procs;
            p = &(procs[ cur_idx ]);
        }

        f = drand48();
        if( f < rw_ratio ) {
            nreads++;
            op='R';
        }
        else{
            nwrites++;
            op='W';
        }

        if( strcmp( ref_pattern, "random" ) == 0 ) {
            addr = (2 * rand()) % max_proc_size ;
        }
        else{
            addr = (p->last_ref + mem_stride ) % max_proc_size ;
        }

        p->last_ref = addr;
        fprintf( stdout, "%u %c 0x%x\n", p->id, op, addr );
    }

    //fprintf(stderr, "nreads: %u, nwrites:  %u, r/w ratio: %.2f\n", nreads, nwrites, ((float)nreads)/(nreads+nwrites) );
    return 0;
}

void proc_args( int iargc,
                char ** iargv )
{
    static struct option long_options[] = {
        {"num-references", 1, NULL, 'r'},
        {"num-processes", 1, NULL, 'p'},
        {"max-process-size", 1, NULL, 'm'},
        {"reference-pattern", 1, NULL, 'f'},
        {"rw-ratio", 1, NULL, 'a'},
        {"mem-stride", 1, NULL, 's'},
        {"time-slice", 1, NULL, 't'},
        {0, 0, 0, 0}
    };
    int option_index = 0;
    int c;
    unsigned int tmp_stride;

    ref_pattern = "sequential";
    rw_ratio = 0.6;
    num_references = 1000000;
    mem_stride = 1024;
    num_procs = 5;
    max_proc_size = 1024*1024*1024;
    slice = 10000;

    while ( 1 ) {
        c = getopt_long ( iargc, iargv, "f:a:p:r:s:m:t:",
                          long_options, &option_index );
        
        if (c == -1)
            break;

        switch (c) {
        case 'f':
            ref_pattern = optarg;
            if( strcmp( ref_pattern, "sequential" ) != 0 &&
                strcmp( ref_pattern, "random" ) != 0 ) {
                fprintf( stderr, "Error: reference-pattern must be \"sequential\" or \"random\"\n");
                exit( -1 );
            }
            break;
        case 'a':
            rw_ratio = atof( optarg );
            break;
        case 'p':
            num_procs = atoi( optarg );
            break;
        case 't':
            slice = atoi( optarg );
            break;
        case 'r':
            num_references = atoi( optarg );
            break;
        case 'm':
            max_proc_size = atoi( optarg );

            /* tmp_stride = max_proc_size;
            while( tmp_stride > 1 ){
                if(tmp_stride % 2 != 0 ){
                    fprintf(stderr, "maximum process size: %u must be a power of 2\n", max_proc_size);
                    exit(-1);
                }
                tmp_stride /= 2;
                } */
            break;
        case 's':
            mem_stride = atoi( optarg );

            /* tmp_stride = mem_stride;
            while( tmp_stride > 1 ){
                if(tmp_stride % 2 != 0 ){
                    fprintf(stderr, "memory stride: %u must be a power of 2\n", mem_stride);
                    exit(-1);
                }
                tmp_stride /= 2;
                }  */
            break;
        default:
            break;
        }
    }
}
