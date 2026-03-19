export type CategoryStatus = 'Active' | 'Inactive';

export interface PagedResult<T> {
  items: T[];
  pageSize: number;
  totalCount: number;
  cursor: string | null;
  nextCursor: string | null;
}

export interface CategoryTreeItem {
  id: number;
  parentCategoryId: number | null;
  name: string;
  description: string | null;
  status: CategoryStatus;
  displayOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  rowVersion: string;
  children: CategoryTreeItem[];
}

export interface ProductListItem {
  id: number;
  name: string;
  description: string | null;
  price: number;
  inventoryOnHand: number;
  primaryImageUrl: string | null;
  versionNumber: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  categoryId: number;
  categoryName: string;
}

export interface ProductDetail extends ProductListItem {
  customAttributes: Record<string, unknown> | null;
  rowVersion: string;
}

export interface ProductAutocompleteItem {
  id: number;
  name: string;
  categoryName: string;
  primaryImageUrl: string | null;
}

export interface ProductWritePayload {
  name: string;
  description: string | null;
  price: number;
  inventoryOnHand: number;
  categoryId: number;
  primaryImageUrl: string | null;
  customAttributes: Record<string, unknown> | null;
  inventoryReason: string | null;
  changedBy: string | null;
}

export interface ProductUpdatePayload extends Partial<ProductWritePayload> {
  rowVersion: string;
}

export interface CategoryWritePayload {
  name: string;
  description: string | null;
  parentCategoryId: number | null;
  status: CategoryStatus;
  displayOrder: number;
}

export interface CategoryUpdatePayload extends Partial<CategoryWritePayload> {
  rowVersion: string;
}
