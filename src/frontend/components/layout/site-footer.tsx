export function SiteFooter() {
  return (
    <footer className="border-t border-slate-200 py-8 dark:border-slate-800">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 text-sm text-slate-500 sm:flex-row sm:px-6 dark:text-slate-400">
        <p>&copy; {new Date().getFullYear()} RealTest. All rights reserved.</p>
        <p>Real people. Real feedback. Real fast.</p>
      </div>
    </footer>
  );
}
