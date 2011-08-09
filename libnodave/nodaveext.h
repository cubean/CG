//#include "nodave.h" 
#define daveBlockTypeNo_ALL  0
#define daveBlockTypeNo_AREA1  1
#define daveBlockTypeNo_OB  8
#define daveBlockTypeNo_DB  10
#define daveBlockTypeNo_SDB 11

#define daveBlockStatus_Active	'A'
#define daveBlockStatus_Passive 'P'
#define daveBlockStatus_MC		'M'

char * DECL2 daveStrerrorExt(int code);
int DECL2 davePutProgramBlock(daveConnection * dc, int blockType, int blockNr, uc* buffer, int length);
int DECL2 daveInsertBlock(daveConnection * dc, int blockType, int blockNr);

typedef struct {
	uc header[36];	
	uc * area1;		
	uc * area3;		
	int hlen;		
	int a1len;		
	int a3len;		
} daveBlock;

typedef int (DECL2 * _foreachInsFunc) (daveInterface * di, char * buf); 

typedef struct {
	int x1;	
}daveAsmnterface;