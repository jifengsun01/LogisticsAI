
type NavItem = {
  id: string;
  label: string;
  icon: React.ReactNode;
};

const icons = {
  dashboard: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={1.8} viewBox="0 0 24 24">
      <rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" />
      <rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" />
    </svg>
  ),
  shipments: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={1.8} viewBox="0 0 24 24">
      <path d="M5 17H3a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11a2 2 0 0 1 2 2v3" />
      <rect x="9" y="11" width="14" height="10" rx="2" />
      <circle cx="12" cy="21" r="1" /><circle cx="20" cy="21" r="1" />
    </svg>
  ),
  delays: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={1.8} viewBox="0 0 24 24">
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </svg>
  ),
  analytics: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={1.8} viewBox="0 0 24 24">
      <line x1="18" y1="20" x2="18" y2="10" /><line x1="12" y1="20" x2="12" y2="4" />
      <line x1="6" y1="20" x2="6" y2="14" />
    </svg>
  ),
  settings: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={1.8} viewBox="0 0 24 24">
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
    </svg>
  ),
};

const mainNav: NavItem[] = [
  { id: 'dashboard',  label: 'Dashboard',  icon: icons.dashboard  },
  { id: 'shipments',  label: 'Shipments',  icon: icons.shipments  },
  { id: 'delays',     label: 'Delays',     icon: icons.delays     },
  { id: 'analytics',  label: 'Analytics',  icon: icons.analytics  },
];

interface SidebarProps {
  active: string;
  onNavigate: (id: string) => void;
  width: number;
}

export default function Sidebar({ active, onNavigate, width }: SidebarProps) {
  const NavButton = ({ item }: { item: NavItem }) => (
    <button
      onClick={() => onNavigate(item.id)}
      className={`
        w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium
        transition-colors duration-150 text-left
        ${active === item.id
          ? 'bg-indigo-600 text-white'
          : 'text-slate-400 hover:bg-slate-700 hover:text-slate-100'}
      `}
    >
      {item.icon}
      {item.label}
    </button>
  );

  return (
    <aside className="h-screen bg-slate-900 flex flex-col px-3 py-4 flex-shrink-0 overflow-hidden"
           style={{ width }}>
      {/* Logo */}
      <div className="px-3 mb-6">
        <h1 className="text-white text-xl font-bold tracking-tight">LogiAI</h1>
        <p className="text-slate-400 text-xs mt-0.5">Logistics assistant</p>
      </div>

      {/* Main nav */}
      <nav className="flex flex-col gap-1 flex-1">
        {mainNav.map(item => <NavButton key={item.id} item={item} />)}
      </nav>

      {/* Settings pinned to bottom */}
      <div className="mt-auto">
        <NavButton item={{ id: 'settings', label: 'Settings', icon: icons.settings }} />
      </div>
    </aside>
  );
}
