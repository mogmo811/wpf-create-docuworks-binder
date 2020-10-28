/* for Adobe use only */
#define _AcroViewHFT_LATEST_VERSION 0x000C0003
#define _AcroViewHFT_LAST_BETA_COMPATIBLE_VERSION 0x000B0000
#define _AcroViewHFT_IS_BETA 1

/* for public use */
#define AcroViewHFT_LATEST_VERSION (_AcroViewHFT_IS_BETA ? (kHFT_IN_BETA_FLAG | _AcroViewHFT_LATEST_VERSION) : _AcroViewHFT_LATEST_VERSION)

#define AcroViewHFT_VERSION_2   	0x00020000
#define AcroViewHFT_VERSION_2_1 	0x00020001
#define AcroViewHFT_VERSION_2_2 	0x00020002
#define AcroViewHFT_VERSION_2_3 	0x00020003
#define AcroViewHFT_VERSION_4   	0x00040000
#define AcroViewHFT_VERSION_4_5 	0x00040005
#define AcroViewHFT_VERSION_5   	0x00050000
#define AcroViewHFT_VERSION_5_1 	0x00050001
#define AcroViewHFT_VERSION_6   	0x00060000
#define AcroViewHFT_VERSION_7		0x00070000
#define AcroViewHFT_VERSION_8   	0x00080000
#define AcroViewHFT_VERSION_9   	0x00090000
#define AcroViewHFT_VERSION_10		0x000A0000
#define AcroViewHFT_VERSION_11  	0x000B0000
#define AcroViewHFT_VERSION_11_6	0x000B0006
#define AcroViewHFT_VERSION_12  	0x000C0000
#define AcroViewHFT_VERSION_12_3	AcroViewHFT_LATEST_VERSION