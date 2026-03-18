<script lang="ts">
  import CategoryNode from './CategoryNode.svelte';
  import type { CategoryTreeItem } from './types';
  import { GitBranch } from 'lucide-svelte';

  interface Props {
    category: CategoryTreeItem;
    currentId: number | null;
    depth?: number;
    onSelect: (category: CategoryTreeItem) => void;
    onCreateChild: (parentCategoryId?: number | null) => void;
  }

  let { category, currentId, depth = 0, onSelect, onCreateChild }: Props = $props();
</script>

<div class="relative w-full">
  <div class="group flex items-center justify-between py-2 px-3 hover:bg-slate-800/50 rounded-lg cursor-pointer transition-colors {currentId === category.id ? 'bg-indigo-500/10 shadow-sm border border-indigo-500/20' : 'border border-transparent'}"
       onclick={() => onSelect(category)}>
    
    <div class="flex items-center gap-3 overflow-hidden" style="padding-left: {depth * 1.5}rem;">
      {#if depth > 0}
        <!-- Tree line visual -->
        <div class="absolute left-0 top-1/2 w-4 border-t border-slate-700/50" style="left: {(depth - 1) * 1.5 + 1}rem;"></div>
        <div class="absolute top-0 bottom-1/2 border-l border-slate-700/50" style="left: {(depth - 1) * 1.5 + 1}rem;"></div>
      {/if}
      
      <div class="relative flex items-center shrink-0">
        {#if category.status === 'Inactive'}
          <div class="w-2.5 h-2.5 rounded-full border-2 border-slate-600 bg-slate-800"></div>
        {:else}
          <div class="w-2.5 h-2.5 rounded-full bg-indigo-500"></div>
        {/if}
      </div>
      
      <div class="flex flex-col">
        <span class="text-sm font-semibold text-slate-200 group-hover:text-white transition-colors">{category.name}</span>
        <span class="text-xs text-slate-500">{category.children.length} children · Ord {category.displayOrder}</span>
      </div>
    </div>

    <!-- Actions, shown on hover -->
    <button 
      title="Add Child Category"
      class="opacity-0 group-hover:opacity-100 p-1.5 text-slate-400 hover:text-indigo-400 hover:bg-slate-800 rounded-md transition-all z-10" 
      onclick={(e) => { e.stopPropagation(); onCreateChild(category.id); }}
    >
      <GitBranch size={16} />
    </button>
  </div>

  {#if category.children.length > 0}
    <div class="relative mt-1">
      {#each category.children as child (child.id)}
        <CategoryNode
          category={child}
          currentId={currentId}
          depth={depth + 1}
          {onCreateChild}
          {onSelect}
        />
      {/each}
      <!-- Vertical line continuing for children -->
      <div class="absolute top-0 bottom-4 border-l border-slate-700/50 pointer-events-none" style="left: {depth * 1.5 + 1}rem;"></div>
    </div>
  {/if}
</div>
