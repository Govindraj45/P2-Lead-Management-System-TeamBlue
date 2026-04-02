export default function Spinner({ size = 'md' }) {
  const sizeClass = size === 'sm' ? 'h-5 w-5' : size === 'lg' ? 'h-12 w-12' : 'h-7 w-7';
  const borderClass = size === 'sm' ? 'border-2' : 'border-[3px]';
  return (
    <div className="flex items-center justify-center p-4">
      <div className={`${sizeClass} animate-spin rounded-full ${borderClass} border-indigo-200 border-t-indigo-600 border-r-purple-500`} />
    </div>
  );
}
