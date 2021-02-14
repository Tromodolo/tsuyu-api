export interface File {
	id: number;
	original_name: string;
	name: string;
	filetype: string;
	file_hash: string;
	file_size: number;
	uploaded_by: number;
	uploaded_by_ip: string;
	created_at: string;
}

export interface FileUpload {
	file: File;
	url: string;
}

export interface FileCount {
	num_count: number;
}

export interface FileState {
	currentPage: number;
	totalPages: number;
	files?: File[];
	recentlyUploaded?: string[];
	uploadProgress?: number;
}