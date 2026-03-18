<script lang="ts">
    import '../app.css';
    import { page } from '$app/stores';
    import { LayoutDashboard, PackageSearch, Folders } from 'lucide-svelte';
</script>

<div class="flex h-screen bg-slate-950 text-slate-100 overflow-hidden font-sans">
    <!-- Sidebar -->
    <aside class="w-64 border-r border-slate-800/50 bg-slate-900/50 backdrop-blur-xl flex flex-col z-20 shrink-0">
        <div class="p-6 border-b border-slate-800/50">
            <h1 class="text-xl font-bold bg-gradient-to-r from-indigo-400 to-cyan-400 bg-clip-text text-transparent tracking-tight">Catalog Studio</h1>
        </div>
        
        <nav class="p-4 flex-1 space-y-2 overflow-y-auto">
            <a href="/" class="flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 {$page.url.pathname === '/' ? 'bg-indigo-500/10 text-indigo-400 shadow-sm' : 'text-slate-400 hover:text-slate-100 hover:bg-slate-800/50'}">
                <LayoutDashboard size={20} strokeWidth={2.5} />
                <span class="font-medium">Dashboard</span>
            </a>
            <a href="/products" class="flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 {$page.url.pathname.startsWith('/products') ? 'bg-indigo-500/10 text-indigo-400 shadow-sm' : 'text-slate-400 hover:text-slate-100 hover:bg-slate-800/50'}">
                <PackageSearch size={20} strokeWidth={2.5} />
                <span class="font-medium">Products</span>
            </a>
            <a href="/categories" class="flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 {$page.url.pathname.startsWith('/categories') ? 'bg-indigo-500/10 text-indigo-400 shadow-sm' : 'text-slate-400 hover:text-slate-100 hover:bg-slate-800/50'}">
                <Folders size={20} strokeWidth={2.5} />
                <span class="font-medium">Categories</span>
            </a>
        </nav>
        
        <div class="p-4 border-t border-slate-800/50 flex items-center gap-3 text-sm text-slate-400">
            <div class="relative flex h-3 w-3">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
              <span class="relative inline-flex rounded-full h-3 w-3 bg-emerald-500"></span>
            </div>
            <span class="font-medium tracking-wide">System Online</span>
        </div>
    </aside>
    
    <!-- Main Content Area -->
    <main class="flex-1 overflow-auto bg-slate-950 relative">
        <!-- Subtle noise background overlay -->
        <div class="absolute inset-0 opacity-[0.03] pointer-events-none" style="background-image: url('data:image/svg+xml,%3Csvg viewBox=%220 0 200 200%22 xmlns=%22http://www.w3.org/2000/svg%22%3E%3Cfilter id=%22noiseFilter%22%3E%3CfeTurbulence type=%22fractalNoise%22 baseFrequency=%220.65%22 numOctaves=%223%22 stitchTiles=%22stitch%22/%3E%3C/filter%3E%3Crect width=%22100%25%22 height=%22100%25%22 filter=%22url(%23noiseFilter)%22/%3E%3C/svg%3E');"></div>
        
        <!-- Glowing orb gradients in background -->
        <div class="absolute top-0 left-1/4 w-96 h-96 bg-indigo-500/10 rounded-full blur-[128px] pointer-events-none"></div>
        <div class="absolute bottom-0 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-[128px] pointer-events-none"></div>

        <div class="relative z-10 w-full h-full p-8 max-w-[1600px] mx-auto">
            <slot />
        </div>
    </main>
</div>
