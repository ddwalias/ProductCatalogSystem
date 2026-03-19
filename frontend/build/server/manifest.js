const manifest = (() => {
function __memo(fn) {
	let value;
	return () => value ??= (value = fn());
}

return {
	appDir: "_app",
	appPath: "_app",
	assets: new Set([]),
	mimeTypes: {},
	_: {
		client: {start:"_app/immutable/entry/start.B__ItMQA.js",app:"_app/immutable/entry/app.Ca1Sq2Ec.js",imports:["_app/immutable/entry/start.B__ItMQA.js","_app/immutable/chunks/BoBO-OUl.js","_app/immutable/chunks/CavPA9og.js","_app/immutable/chunks/B5lqAbQJ.js","_app/immutable/entry/app.Ca1Sq2Ec.js","_app/immutable/chunks/B5lqAbQJ.js","_app/immutable/chunks/DsnmJJEf.js","_app/immutable/chunks/CavPA9og.js","_app/immutable/chunks/T8X6bnje.js"],stylesheets:[],fonts:[],uses_env_dynamic_public:false},
		nodes: [
			__memo(() => import('./chunks/0-DCC6hLq9.js')),
			__memo(() => import('./chunks/1-Bku2Ut4A.js')),
			__memo(() => import('./chunks/2-BQjv1CAP.js')),
			__memo(() => import('./chunks/3-DOVul3QC.js')),
			__memo(() => import('./chunks/4-BqgmHD_Z.js'))
		],
		remotes: {
			
		},
		routes: [
			{
				id: "/",
				pattern: /^\/$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 2 },
				endpoint: null
			},
			{
				id: "/categories",
				pattern: /^\/categories\/?$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 3 },
				endpoint: null
			},
			{
				id: "/products",
				pattern: /^\/products\/?$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 4 },
				endpoint: null
			}
		],
		prerendered_routes: new Set([]),
		matchers: async () => {
			
			return {  };
		},
		server_assets: {}
	}
}
})();

const prerendered = new Set([]);

const base = "";

export { base, manifest, prerendered };
//# sourceMappingURL=manifest.js.map
